# .NET 微服務 + 事件溯源模板

這是一個以 .NET 為核心的微服務模板，內建 API Gateway、CQRS 與事件溯源基礎設計，目標是提供一個可快速複製的專案骨架，包含常見服務切分、共用元件、測試結構與 Docker Compose 配置。

## 專案定位

- 提供微服務 + CQRS + 事件溯源的專案起手式
- 以 API Gateway 統一路由與限流
- 服務依職責切分（登入、註冊、角色權限、控制台等）
- 共享共通元件（Helper、Common Library、共享模型）

## 架構概覽

- **API Gateway**：`GateWay` 使用 Ocelot，支援路由、快取、CORS、Forwarded Headers 與 Rate Limiter。
- **CQRS / Event Sourcing Core**：`CQRS.Core` 放置事件與指令相關抽象層與基礎型別。
- **共用函式庫**：`CommonLibrary` 提供跨服務可共用的 Helper / Extension。
- **資料存取**：`DataAccess` 統一 DB 連線與 Repository、Unit of Work 的基礎抽象。
- **服務專案**：`Services/*` 以不同業務域拆分 CMD/QUERY 專案。
- **測試專案**：`Tests/*` 統一測試規範與 xUnit Runner 設定。

## 目錄結構（WebApi）

- `GateWay/`：API Gateway（Ocelot）
- `CQRS.Core/`：事件溯源與 CQRS 核心抽象
- `CommonLibrary/`：共用 Helpers / Extensions
- `DataAccess/`：資料庫連線與倉儲/工作單元基礎
- `Services/`：各微服務主體
- `Tests/`：測試專案與測試規範

## 服務清單（Services）

以下為已建立的服務範例（依資料夾名稱）：

- `ControllerService`
- `LogInService`
- `SignUpService`
- `RoleService`
- `RolePermissionService`
- `MailService`
- `CommonFilesManagement`
- `Service.Common`
- `Service.Background`
- `Services.Shared`

> 每個服務可依需求拆分為 Command / Query 專案，並由 Gateway 統一路由轉發。

## Docker Compose 概覽

`WebApi/docker-compose.yml` 已預設多服務映像與環境變數配置，包含：

- Gateway
- Controller / Login / Signup / Role / RolePermission 之 CMD/Query 服務
- Redis 連線字串（由環境變數注入）
- LaunchDarkly SDK Key（僅 Query 服務）

你可以依實際環境調整：

- `HARBOR_REGISTRY` / `HARBOR_PROJECT`
- 服務版本 Tag（如 `systemAPI_*_TAG`）
- `ASPNETCORE_ENVIRONMENT`

## 快速開始

1. 還原相依套件

	```bash
	dotnet restore WebApi/SystemMain_WebApi.slnx
	```

2. 啟動 Gateway 或任一服務（示意）

	```bash
	dotnet run --project WebApi/GateWay/GateWay.csproj
	```

3. 使用 Docker Compose 啟動（需先準備映像）

	```bash
	docker compose -f WebApi/docker-compose.yml up
	```

## 測試

測試專案集中於 `WebApi/Tests`，並使用集中式 `Directory.Build.props` 管理。規範摘要：

- 測試專案透過 `ProjectReference` 參考對應服務
- 若需要 `Microsoft.AspNetCore.Mvc.Testing`，僅加在該測試專案
- xUnit Runner 設定遵循覆蓋優先序

## 使用建議

- 新增服務時優先參考 `Services/` 現有結構
- 事件與指令型別應統一放置於 `CQRS.Core`
- 新增路由時更新 `GateWay/ocelot.json`

## 版本與環境

- .NET 版本：依各專案 `.csproj` 為準
- Docker：支援多服務部署情境

---

此專案為模板用途，建議依實際業務需求調整服務拆分、事件模型與資料庫策略。
