---
name: work-log-reporter
description: "Use when: 記錄工作日報, 寫到 Google Sheet, 工作紀錄, commit 記錄到試算表, work log, 日報, 工作日誌, log today's work, 把 commit 寫進日報, commit 並寫日報, commit 完後記日報"
---

# Work Log Reporter

分析目前的程式碼變更，依序完成「產生 commit message」→「產生工作日誌」→「寫入 Google Sheet」→「執行 git commit」，每個階段都需使用者確認後才繼續。

---

## 前置需求

- Google Sheets MCP Server 已安裝（`.mcp-servers/google-sheets/`，見 README）
- 首次使用請先執行 `mcp_google_sheets_init_work_log_sheet` 初始化分頁

---

## 工作流程

> **重要原則**：每個 Phase 完成後必須停下來等使用者確認，確認後才繼續下一個 Phase。

---

### Phase 1：分析變更並產生 Commit Message

#### 1-1. 取得 staged 變更

```bash
git status --short
```

若尚未 stage 任何檔案，先執行：

```bash
git add -A
```

再取得詳細 diff：

```bash
git diff --cached
git diff --cached --stat
```

#### 1-2. 分析變更語意

分析時注意：
1. **哪些檔案被異動**（新增 / 修改 / 刪除）
2. **異動的層級**：Controller、Command/Query、Domain、Infrastructure、Tests、Config
3. **異動的業務語意**：新增功能、修正 bug、重構、測試、文件、設定調整
4. **是否跨越多個服務／模組**（若是，message 要適度概括）

#### 1-3. 產生 Conventional Commits 格式的 Commit Message

格式：

```
<type>(<scope>): <subject>

[body]
```

**Type 對照表：**

| Type | 使用時機 |
|------|----------|
| `feat` | 新增功能 |
| `fix` | 修正 bug |
| `refactor` | 重構，不影響功能 |
| `test` | 新增或修改測試 |
| `docs` | 文件修改 |
| `chore` | 建置設定、雜務 |
| `style` | 格式調整 |
| `perf` | 效能改善 |
| `build` | 建置系統相關 |
| `revert` | 還原先前 commit |

**Scope 慣例：** `controller-service` / `login-service` / `signup-service` / `role-service` / `role-permission-service` / `gateway` / `cqrs-core` / `data-access` / `common` / `tests` / `infra`

**Subject 規則：** 繁體中文、動詞開頭、不加句號、≤ 72 字元

#### 1-4. 顯示並等待確認

以以下格式呈現，**然後停下來，詢問使用者是否確認** ：

```
## 📝 Commit Message

**異動檔案：**
- `path/to/file.cs`（修改）
- ...

---

**建議的 Commit Message：**

\`\`\`
feat(scope): subject line

body 說明（若有）
\`\`\`

---
請確認 commit message 是否正確？如需修改請告知，確認後我會繼續產生工作日誌。
```

> ⛔ **等使用者回覆「確認」或「OK」後才進行 Phase 2，不得自動繼續。**

---

### Phase 2：產生工作日誌預覽

使用者確認 commit message 後，依以下規則產生工作日誌。

#### 2-1. 取得今天日期

- **date**：今天的 `YYYY/M/D`（例如 `2026/3/9`）

#### 2-2. 取得異動檔案數

由 Phase 1 的 `git diff --cached --stat` 輸出中，取最後一行的檔案數字（例如 `5 files changed`）作為參考資訊（**不寫入 Sheet**）。

#### 2-3. Google Sheet 欄位對應規則

> **重要**：MCP 工具的參數名稱與 Google Sheet 實際欄位意義不同，需依以下對應填入。
> 欄位定義來自實際讀取 Google Sheet 資料，**不可自行假設**。

| Google Sheet 欄 | MCP 參數 | 填入內容 | 說明 |
|----------------|---------|---------|------|
| A：日期 | `date` | `YYYY/M/D` | 今天日期，使用 `/` 分隔，月日不補零 |
| B：工作項目 | `workItem` | 工作項目名稱 | subject 的描述部分（去除 `type(scope):` 前綴），若無符合格式則填 subject 原文 |
| C：工作說明 | `workDescription` | 工作說明（可多行） | 完整 commit 標題或詳細說明，多筆用換行分隔 |
| D：完成率 | `completionRate` | 完成率百分比 | 詢問使用者完成率，如 `100%`、`57%` |
| E：完成日 | `completionDate` | `YYYY/M/D` | 詢問使用者，預設與日期相同 |
| F：備注 | `remark` | 備注文字 | 選填，通常留空 |


#### 2-4. 顯示工作日誌預覽並等待確認

以 Markdown 表格呈現，**然後停下來，詢問使用者是否確認**：

```
## 📋 工作日誌預覽

| 日期      | 工作項目（欄B）        | 工作說明（欄C）                          | 完成率 | 完成日    |
|-----------|----------------------|-----------------------------------------|--------|----------|
| 2026/3/9  | 新增 OAuth 登入功能   | feat(login-service): 新增 OAuth 登入功能 | 100%   | 2026/3/9  |

---
請確認工作日誌內容是否正確？確認後我會寫入 Google Sheet，然後執行 git commit。
```

> ⛔ **等使用者回覆「確認」或「OK」後才進行 Phase 3，不得自動繼續。**

---

### Phase 3：寫入 Google Sheet

使用者確認工作日誌後，執行以下步驟：

#### 3-1. 載入並確認 MCP 工具

使用 `tool_search_tool_regex` 確認工具已載入：

```
pattern: "append_work_log|init_work_log|read_work_log"
```

若搜尋結果為空，提示使用者：
1. 確認已執行 `npm install`（在 `.mcp-servers/google-sheets/` 下）
2. 在 VS Code 重新載入 MCP Server（Command Palette → **MCP: Restart Server**）

#### 3-2. 呼叫 `mcp_google_sheets_append_work_log`

依 Phase 2 的欄位對應規則填入參數：

```json
{
  "date": "2026/3/9",
  "workItem": "新增 OAuth 登入功能",
  "workDescription": "feat(login-service): 新增 OAuth 登入功能",
  "completionRate": "100%",
  "completionDate": "2026/3/9",
  "remark": ""
}
```

---

### Phase 4：執行 Git Commit

Google Sheet 寫入成功後，立即執行 git commit：

```bash
git commit -m "<subject>" -m "<body（若有）>"
```

若 message 有多行 body，使用多個 `-m` 參數：

```bash
git commit -m "feat(scope): subject line" -m "Body content here."
```

#### 完成後輸出摘要

```
✅ 工作日誌已寫入 Google Sheet，git commit 已完成

**Commit：** feat(login-service): 新增 OAuth 登入功能
**Hash：** a1b2c3d
**工作項目（workItem）：** 新增 OAuth 登入功能
**完成日（completionDate）：** 2026/3/9
```

> **注意：不執行 `git push`**，push 需另行由使用者確認。

---

## 查看最近記錄

若使用者想查看 Google Sheet 上的最近工作日報，呼叫 `mcp_google_sheets_read_work_log`：

```json
{ "limit": 10 }
```

並以表格格式顯示結果。
