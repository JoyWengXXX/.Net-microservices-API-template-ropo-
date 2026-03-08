# Google Sheets Work Log — MCP Server

透過 **Google Service Account** 認證，將 git commit 資訊寫入 Google Sheet 作為工作日報。

---

## 前置需求

- Node.js v18+
- 一個 Google Cloud 專案，並啟用 **Google Sheets API**
- 一個已建立的 Google Spreadsheet

---

## Step 1：建立 Google Service Account

1. 前往 [Google Cloud Console](https://console.cloud.google.com/) → 選擇（或建立）你的專案
2. 左側選單 → **APIs & Services** → **Library** → 搜尋 **Google Sheets API** → **Enable**
3. 左側選單 → **APIs & Services** → **Credentials** → **Create Credentials** → **Service account**
4. 填入名稱（例如 `worklog-bot`），按 **Done**
5. 點擊剛建立的 Service Account → 上方 **Keys** 分頁 → **Add Key** → **Create new key** → **JSON**
6. 下載的 JSON 檔案即為 credentials，**妥善保存，不要 commit 到 repo**

---

## Step 2：共用 Google Spreadsheet 給 Service Account

1. 開啟目標 Google Spreadsheet
2. 右上角 **Share** → 貼入 Service Account 的 email（格式：`your-name@your-project.iam.gserviceaccount.com`）
3. 角色設為 **Editor** → 送出

---

## Step 3：取得 Spreadsheet ID

Spreadsheet URL 格式：
```
https://docs.google.com/spreadsheets/d/<SPREADSHEET_ID>/edit
```
複製 `<SPREADSHEET_ID>` 備用。

---

## Step 4：安裝相依套件

在 MCP Server 目錄執行：

```bash
cd .mcp-servers/google-sheets
npm install
```

---

## Step 5：設定 VS Code MCP 輸入

首次啟動 MCP Server 時，VS Code 會依序詢問三個輸入：

| 輸入提示 | 說明 | 範例 |
|----------|------|------|
| Google Service Account credentials JSON 絕對路徑 | Step 1 下載的 JSON 金鑰檔完整路徑 | `C:\secrets\worklog-sa.json` |
| Google Spreadsheet ID | Step 3 取得的 ID | `1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms` |
| Work log Sheet 名稱 | 分頁名稱（預設 `WorkLog`） | `WorkLog` |

---

## 支援的 MCP 工具

| 工具名稱 | 說明 |
|----------|------|
| `mcp_google_sheets_init_work_log_sheet` | 初始化工作日報分頁（建立標題列） |
| `mcp_google_sheets_append_work_log` | 新增一筆工作日報記錄 |
| `mcp_google_sheets_read_work_log` | 讀取最近的工作日報記錄 |

---

## Google Sheet 欄位結構

| A: 日期 | B: 星期 | C: Commit Hash | D: Type | E: Scope | F: 工作摘要 | G: 異動檔案數 | H: Commit 完整訊息 |
|---------|---------|----------------|---------|----------|------------|--------------|-------------------|

---

## 安全注意事項

- credentials JSON **絕對不能** 放入 repo 或 commit
- 已於根目錄 `.gitignore` 排除 `*.json` Service Account 金鑰（請自行確認）
- Service Account 僅開啟 Sheets API 範圍，不授予其他權限
