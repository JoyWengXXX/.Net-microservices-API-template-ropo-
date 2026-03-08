# 後端開發指引 — .NET Microservices API

## 專案架構

本專案為 .NET 微服務架構，採用 **CQRS + Event Sourcing** 模式。

- **GateWay**: Ocelot API Gateway，負責路由與 JWT 驗證
- **Services/**: 各微服務，每個服務拆分為 Cmd（寫入）與 Query（讀取）兩個 API 專案
- **CQRS.Core**: CQRS 基礎建設（Dispatcher、EventStore、AggregateRoot）
- **DataAccess**: Repository Pattern、UnitOfWork
- **DBContexts**: Entity Framework Core DbContext
- **CommonLibrary**: 共用 Helpers
- **Tests/**: 所有單元測試，使用 xUnit + Moq

每個服務的結構：
```
ServiceName/
  ServiceName.Cmd.API/         ← Command side (寫入 API)
  ServiceName.Cmd.Domain/      ← Aggregate、Events、DTOs
  ServiceName.Cmd.Infrastructure/ ← EventHandler（處理事件寫入 DB）
  ServiceName.Query.API/       ← Query side (讀取 API)
  ServiceName.Query.Domain/    ← Queries
  ServiceName.Query.Infrastructure/ ← QueryHandler（從 DB 讀取）
```

## 必讀 Instructions

詳細規範請參見：
- [C# 編碼風格](.github/instructions/csharp-style.instructions.md) — 命名、格式、語法慣例
- [CQRS 架構模式](.github/instructions/cqrs-microservices.instructions.md) — Command/Event/Aggregate 實作模式
- [TDD 與測試規範](.github/instructions/testing.instructions.md) — 單元測試與整合測試規則

## 核心原則

1. **永遠使用 async/await**，不使用 `.Result` 或 `.Wait()`
2. 所有 Command/Query Handler 回傳 `TResult`
3. 依賴注入：建構子注入，`Scoped` 為預設生命週期
4. 路由慣例：`api/v1/[controller]`，HTTP 動詞對應語義操作
5. **每個新功能必須附帶對應的單元測試**，遵循 Arrange / Act / Assert 模式

## 建置與測試

```bash
dotnet build SystemMain_WebApi.slnx
dotnet test Tests/Tests.slnx
```

測試專案位於 `Tests/`，使用 `Tests.slnx` solution file。

## Git Commit 工作流程

使用 **Git Commit** agent 自動分析變更並產生符合 Conventional Commits 規範的 commit message：

- Agent 檔案：`.github/agents/git-commit.agent.md`
- 呼叫方式：在 Chat 輸入 `@Git Commit` 或說「幫我 commit」
- Agent 會自動執行 `git add -A`、分析 diff、產生 message，並在確認後執行 `git commit`
- **不會自動 push**，push 需另行確認

> **重要**：當使用者請求 git commit（包含「git commit」、「幫我 commit」、「提交程式碼」等），無論是否透過 subagent，都**必須先讀取 `.github/agents/git-commit.agent.md`**，依照其中的規範（包含繁體中文 subject、Conventional Commits 格式、scope 命名等）產生 commit message，再執行。

## 工作日報工作流程

使用 **work-log-reporter** skill 將 git commit 自動轉換為 Google Sheet 工作日報：

- Skill 檔案：`.github/skills/work-log/SKILL.md`
- 觸發方式：說「記錄工作日報」、「寫到 Google Sheet」、「寫日報」或「commit 完後記日報」
- MCP Server：`.mcp-servers/google-sheets/`（須先 `npm install` 並完成 Google 認證設定）
- Skill 會自動讀取最新 commit、解析 Conventional Commits 格式，並逐筆寫入 Google Sheet

> **首次使用**：請先閱讀 `.mcp-servers/google-sheets/README.md` 完成 Service Account 設定，再呼叫 `mcp_google_sheets_init_work_log_sheet` 初始化分頁。
