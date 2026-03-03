# .NET 微服務 + 事件溯源模板

以 **.NET 9.0** 為核心的微服務架構模板，內建 API Gateway、CQRS 與事件溯源設計，提供可快速複製的專案骨架，涵蓋業務服務切分、共用元件、測試結構與 Docker Compose 完整配置。

## 專案定位

- 微服務 + CQRS + 事件溯源的生產就緒起手式
- API Gateway 統一路由、快取、CORS 與速率限制
- 服務依業務域切分，每個域獨立 CMD / Query 專案
- 共享基礎設施（JWT、Serilog、Feature Flag、gRPC）
- 集中式套件版本管理（`Directory.Packages.props`）

## 架構概覽

```
Client
  └─► GateWay (Ocelot, port 80/443)
        ├─► ControllerService.Cmd  (5126)
        ├─► ControllerService.Query (5044)
        ├─► LogInService.Cmd       (5246)
        ├─► SignUpService.Cmd      (5215)
        ├─► SignUpService.Query    (5289)
        ├─► RoleService.Cmd        (5121)
        ├─► RoleService.Query      (5128)
        ├─► RolePermissionService.Cmd   (5114)
        └─► RolePermissionService.Query (5256)
```

## 目錄結構（WebApi/）

```
WebApi/
├── GateWay/                  # API Gateway（Ocelot 24）
├── CQRS.Core/                # 事件溯源 & CQRS 核心抽象
├── CommonLibrary/            # 跨服務共用 Helpers / Extensions
├── DataAccess/               # DB 連線與 Repository / UoW 抽象
├── DBContexts/SystemMain/    # EF Core DbContext（SQL Server + Event Sourcing DB）
├── Services/                 # 各業務微服務（見下方服務清單）
├── Tests/                    # xUnit 測試專案
├── Directory.Build.props     # 全域專案屬性（net9.0）
├── Directory.Packages.props  # 集中式 NuGet 版本管理
├── docker-compose.yml
└── SystemMain_WebApi.slnx
```

**服務清單（`Services/`）**

每個業務服務依需求拆分為 `{Service}.Cmd` / `{Service}.Query` 兩側，各含 `API`、`Domain`、`Infrastructure` 三層。

| 服務資料夾 | Cmd | Query |
|-----------|:---:|:-----:|
| `ControllerService` | ✅ | ✅ |
| `LogInService` | ✅ | — |
| `SignUpService` | ✅ | ✅ |
| `RoleService` | ✅ | ✅ |
| `RolePermissionService` | ✅ | ✅ |
| `MailService`（gRPC） | ✅ | — |
| `Service.Common` | 跨服務共用基礎設施（JWT、Serilog、Middleware） |
| `Service.Background` | Worker Service（RabbitMQ、DB 監控） |
| `Services.Shared.FeatureFlag` | LaunchDarkly / OpenFeature 整合 |
| `CommonFilesManagement` | 共用檔案存取管理 |

## 主要技術棧

| 類別 | 套件 |
|------|------|
| Gateway | Ocelot 24、Ocelot.Cache.CacheManager |
| ORM | Entity Framework Core 9、Dapper 2 |
| 資料庫 | SQL Server（`Microsoft.Data.SqlClient`）、PostgreSQL（Npgsql 9） |
| 訊息佇列 | RabbitMQ.Client 7 |
| 快取 | StackExchange.Redis 2 |
| 日誌 | Serilog（Elasticsearch、Seq sinks） |
| gRPC | Grpc.AspNetCore 2.62 |
| 認證 | JWT Bearer 9、Google OAuth（`Google.Apis.Auth`） |
| Feature Flag | LaunchDarkly.ServerSdk 8、OpenFeature 2 |
| 測試 | xUnit v3、Moq、Shouldly、coverlet |
| 其他 | AutoMapper 13、AWS SDK（Core、IoT）、LinqKit |

## Docker Compose 服務對應

| Container | 映像 | Host Port |
|-----------|------|-----------|
| `systemapi_gateway` | `systemapi_gateway` | 80 / 443 |
| `systemapi_controller_cmd` | `systemapi_controller_cmd` | 5126 |
| `systemapi_controller_query` | `systemapi_controller_query` | 5044 |
| `systemapi_login_cmd` | `systemapi_login_cmd` | 5246 |
| `systemapi_signup_cmd` | `systemapi_signup_cmd` | 5215 |
| `systemapi_signup_query` | `systemapi_signup_query` | 5289 |
| `systemapi_rolepermission_cmd` | `systemapi_rolepermission_cmd` | 5114 |
| `systemapi_rolepermission_query` | `systemapi_rolepermission_query` | 5256 |
| `systemapi_role_cmd` | `systemapi_role_cmd` | 5121 |
| `systemapi_role_query` | `systemapi_role_query` | 5128 |

> Redis 位於獨立 VM，透過 `redis.local:6379` 連線，不在此 Compose 管理範圍。

環境變數設定（`.env` 或 CI/CD 注入）：

```env
HARBOR_REGISTRY=<your-registry>
HARBOR_PROJECT=<your-project>
ASPNETCORE_ENVIRONMENT=Production
systemAPI_GATEWAY_TAG=latest
systemAPI_CONTROLLER_CMD_TAG=latest
# ... 其他服務 Tag
```

## 快速開始

1. **還原相依套件**

   ```bash
   dotnet restore WebApi/SystemMain_WebApi.slnx
   ```

2. **本機啟動 Gateway**

   ```bash
   dotnet run --project WebApi/GateWay/GateWay.csproj
   ```

3. **本機啟動任一服務（示意）**

   ```bash
   dotnet run --project WebApi/Services/ControllerService/ControllerService.Cmd.API/ControllerService.Cmd.API.csproj
   ```

4. **Docker Compose 啟動（需先備妥映像）**

   ```bash
   docker compose -f WebApi/docker-compose.yml up
   ```

## 測試

測試專案集中於 `WebApi/Tests/`，使用集中式 `Directory.Build.props` 管理共用設定。

**最小測試專案範本：**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\Services\SomeService\SomeProject.csproj" />
  </ItemGroup>
</Project>
```

**重要規則：**
- 整合測試若需要 `Microsoft.AspNetCore.Mvc.Testing`，僅加在該測試專案，不放入共用 `Directory.Build.props`
- xUnit Runner 設定優先序：專案級 `<AssemblyName>.xunit.runner.json` > 專案根 `xunit.runner.json` > `Tests/xunit.runner.json`（共用預設）

## 開發指南

- 新增業務服務時，參考 `ControllerService` 的 Cmd/Query 三層結構
- 事件與指令型別繼承自 `CQRS.Core` 的 `BaseEvent` / `BaseCommand`
- 新路由需同步更新 `GateWay/ocelot.json`（開發環境使用 `ocelot.Dev.json`）
- 共用授權、日誌邏輯統一放置於 `Service.Common`
- Feature Flag 整合請使用 `Services.Shared.FeatureFlag`，避免直接耦合 LaunchDarkly SDK

## 版本與環境

- **目標框架**：.NET 9.0（`Directory.Build.props` 統一設定）
- **Nullable / ImplicitUsings**：全域啟用
- **套件版本**：集中管理於 `Directory.Packages.props`

---

## AI 開發工具（GitHub Copilot）

本專案在 `.github/` 中內建了一套 GitHub Copilot 輔助開發配置，涵蓋 Agents、Prompts、Skills 與 Instructions。

### Agents（`.github/agents/`）

在 Chat 中以 `@` 呼叫，或透過對應 Prompt 自動觸發。

| 檔案 | Agent 名稱 | 用途 |
|------|-----------|------|
| `db-explorer.agent.md` | **DB Explorer** | 唯讀查詢 PostgreSQL 資料庫（表結構、資料預覽），限制 SELECT |
| `git-commit.agent.md` | **Git Commit** | 分析變更、產生 Conventional Commits 格式 message 並自動執行 commit |
| `spec-writer.agent.md` | **Spec Writer** | 將功能簡述轉換為結構化 Feature Spec 文件（`docs/specs/`） |
| `tdd-developer.agent.md` | **TDD Developer** | TDD 紅綠重構循環，專注於 EventHandler / QueryHandler 單元測試 |
| `tech-lead.agent.md` | **Tech Lead** | Code Review：正確性、安全性（OWASP）、效能、SOLID 原則 |
| `tech-researcher.agent.md` | **Tech Researcher** | 查詢官方文件、研究最新 .NET 技術與 AI Coding 趨勢 |

### Prompts（`.github/prompts/`）

在 Chat 中以 `/` 呼叫。

| 檔案 | Prompt 名稱 | 對應 Agent | 用途 |
|------|------------|-----------|------|
| `generate-unit-tests.prompt.md` | **Generate Unit Tests** | TDD Developer | 為指定 Handler 或 Service 類別產生完整單元測試 |
| `integration-test-flow.prompt.md` | **Integration Test Flow** | — | 產生跨服務整合測試流程圖與測試方案 |
| `write-feature-spec.prompt.md` | **Write Feature Spec** | Spec Writer | 將功能簡述輸出為完整 Feature Spec 文件 |

### Skills（`.github/skills/`）

| 資料夾 | Skill 名稱 | 用途 |
|--------|-----------|------|
| `new-microservice/` | **new-microservice** | 依 CQRS 架構一次搭建完整微服務骨架（Cmd/Query 兩側三層 + 測試） |

### Instructions（`.github/instructions/`）

自動套用至符合 `applyTo` 規則的檔案，無需手動呼叫。

| 檔案 | applyTo | 說明 |
|------|---------|------|
| `cqrs-microservices.instructions.md` | `Services/**/*.cs` | CQRS + Event Sourcing 架構模式規範 |
| `csharp-style.instructions.md` | `**/*.cs` | C# 命名慣例與編碼風格規範 |
| `testing.instructions.md` | `Tests/**/*.cs` | TDD 與 xUnit + Moq 測試規範 |

### MCP Servers（`.vscode/mcp.json`）

| Server | 說明 |
|--------|------|
| `postgres` | 連線 PostgreSQL，供 **DB Explorer** agent 查詢資料庫（連線字串在啟動時提示輸入） |
| `fetch` | 抓取網頁內容，供 **Tech Researcher** agent 查詢官方文件與外部資源 |

此專案為模板用途，建議依實際業務需求調整服務拆分、事件模型與資料庫策略。
