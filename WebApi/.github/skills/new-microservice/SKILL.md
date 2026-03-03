---
name: new-microservice
description: "Use when: 新增微服務, scaffold new microservice, create new service, 建立新服務骨架, new CQRS service, add service to project"
---

# New Microservice Scaffold

依照本專案的 CQRS 架構，一手產生一個完整微服務的所有程式碼骨架，包含 Cmd/Query 兩側 API、Domain、Infrastructure，以及對應的單元測試。

## Step 1: 收集必要資訊

向使用者確認以下參數（若未提供則詢問）：

| 參數 | 說明 | 範例 |
|------|------|------|
| `ServiceName` | 服務名稱（PascalCase）| `ProductService` |
| `EntityName` | 主要領域實體名稱（PascalCase）| `Product` |
| `entityName` | 實體名稱（camelCase）| `product` |
| `EntityId` | 主鍵欄位名稱（PascalCase）| `ProductId` |
| `entityId` | 主鍵欄位名稱（camelCase）| `productId` |
| `CmdPort` | Cmd API 的 port | `5331` |
| `QueryPort` | Query API 的 port | `5332` |
| `AuthorizeRole` | 預設授權角色 | `Admin` |

## Step 2: 建立目錄結構

建立以下資料夾結構（所有 `{{ServiceName}}` 替換為實際名稱）：

```
Services/{{ServiceName}}/
  {{ServiceName}}.Cmd.API/
    Commands/
      Interfaces/
    Controllers/
    Properties/
  {{ServiceName}}.Cmd.Domain/
    Aggregates/
    DTOs/
    Events/
    Handlers/
  {{ServiceName}}.Cmd.Infrastructure/
    Handlers/
  {{ServiceName}}.Query.API/
    Controllers/
    Properties/
  {{ServiceName}}.Query.Domain/
    DTOs/
    Mappers/
    Queries/
      Interfaces/
  {{ServiceName}}.Query.Infrastructure/
    Handlers/
Tests/{{ServiceName}}.Test/
```

## Step 3: 產生所有程式碼檔案

依序讀取 `templates/` 資料夾下的各個範本檔，將所有佔位符號替換後寫出目標檔案。

### 佔位符號替換對照表

| 佔位符 | 替換為 |
|--------|--------|
| `{{ServiceName}}` | 服務名稱（PascalCase，e.g. `ProductService`）|
| `{{EntityName}}` | 實體名稱（PascalCase，e.g. `Product`）|
| `{{entityName}}` | 實體名稱（camelCase，e.g. `product`）|
| `{{EntityId}}` | 主鍵名稱（PascalCase，e.g. `ProductId`）|
| `{{entityId}}` | 主鍵名稱（camelCase，e.g. `productId`）|
| `{{CmdPort}}` | Cmd API port（e.g. `5331`）|
| `{{QueryPort}}` | Query API port（e.g. `5332`）|
| `{{AuthorizeRole}}` | 授權角色（e.g. `Admin`）|

### 檔案輸出對照表

| 範本來源 | 輸出路徑 |
|----------|----------|
| `templates/Cmd.API/Program.cs.template` | `Services/{{ServiceName}}/{{ServiceName}}.Cmd.API/Program.cs` |
| `templates/Cmd.API/Controller.cs.template` | `Services/{{ServiceName}}/{{ServiceName}}.Cmd.API/Controllers/{{EntityName}}_CmdController.cs` |
| `templates/Cmd.API/Commands/AddCommand.cs.template` | `...Commands/Add{{EntityName}}Command.cs` |
| `templates/Cmd.API/Commands/UpdateCommand.cs.template` | `...Commands/Update{{EntityName}}Command.cs` |
| `templates/Cmd.API/Commands/DisableCommand.cs.template` | `...Commands/Disable{{EntityName}}Command.cs` |
| `templates/Cmd.API/Commands/CommandHandler.cs.template` | `...Commands/CommandHandler.cs` |
| `templates/Cmd.API/Commands/ICommandHandler.cs.template` | `...Commands/Interfaces/ICommandHandler.cs` |
| `templates/Cmd.API/Cmd.API.csproj.template` | `...{{ServiceName}}.Cmd.API.csproj` |
| `templates/Cmd.API/appsettings.json.template` | `...appsettings.json` |
| `templates/Cmd.API/appsettings.Development.json.template` | `...appsettings.Development.json` |
| `templates/Cmd.Domain/Aggregate.cs.template` | `...Aggregates/{{EntityName}}Aggregate.cs` |
| `templates/Cmd.Domain/Events/AddEvent.cs.template` | `...Events/Add{{EntityName}}Event.cs` |
| `templates/Cmd.Domain/Events/UpdateEvent.cs.template` | `...Events/Update{{EntityName}}Event.cs` |
| `templates/Cmd.Domain/Events/DisableEvent.cs.template` | `...Events/Disable{{EntityName}}Event.cs` |
| `templates/Cmd.Domain/DTOs/AddDTO.cs.template` | `...DTOs/Add{{EntityName}}DTO.cs` |
| `templates/Cmd.Domain/DTOs/UpdateDTO.cs.template` | `...DTOs/Update{{EntityName}}DTO.cs` |
| `templates/Cmd.Domain/IEventHandler.cs.template` | `...Handlers/IEventHandler.cs` |
| `templates/Cmd.Domain/Cmd.Domain.csproj.template` | `...{{ServiceName}}.Cmd.Domain.csproj` |
| `templates/Cmd.Infrastructure/EventHandler.cs.template` | `...Handlers/EventHandler.cs` |
| `templates/Cmd.Infrastructure/EventSourcingHandler.cs.template` | `...Handlers/EventSourcingHandler.cs` |
| `templates/Cmd.Infrastructure/Cmd.Infrastructure.csproj.template` | `...{{ServiceName}}.Cmd.Infrastructure.csproj` |
| `templates/Query.API/Program.cs.template` | `Services/{{ServiceName}}/{{ServiceName}}.Query.API/Program.cs` |
| `templates/Query.API/Controller.cs.template` | `...Controllers/{{EntityName}}_QueryController.cs` |
| `templates/Query.API/Query.API.csproj.template` | `...{{ServiceName}}.Query.API.csproj` |
| `templates/Query.API/appsettings.json.template` | `...appsettings.json` |
| `templates/Query.API/appsettings.Development.json.template` | `...appsettings.Development.json` |
| `templates/Query.Domain/Query.cs.template` | `...Queries/Get{{EntityName}}sQuery.cs` |
| `templates/Query.Domain/IQueryHandler.cs.template` | `...Queries/Interfaces/IQueryHandler.cs` |
| `templates/Query.Domain/GetDTO.cs.template` | `...DTOs/Get{{EntityName}}DTO.cs` |
| `templates/Query.Domain/Mapper.cs.template` | `...Mappers/{{EntityName}}Profile.cs` |
| `templates/Query.Domain/Query.Domain.csproj.template` | `...{{ServiceName}}.Query.Domain.csproj` |
| `templates/Query.Infrastructure/QueryHandler.cs.template` | `...Handlers/QueryHandler.cs` |
| `templates/Query.Infrastructure/Query.Infrastructure.csproj.template` | `...{{ServiceName}}.Query.Infrastructure.csproj` |
| `templates/Tests/GlobalUsings.cs.template` | `Tests/{{ServiceName}}.Test/GlobalUsings.cs` |
| `templates/Tests/EventHandlerTests.cs.template` | `Tests/{{ServiceName}}.Test/EventHandlerTests.cs` |
| `templates/Tests/QueryHandlerTests.cs.template` | `Tests/{{ServiceName}}.Test/QueryHandlerTests.cs` |
| `templates/Tests/Tests.csproj.template` | `Tests/{{ServiceName}}.Test/{{ServiceName}}.Tests.csproj` |

## Step 4: 更新 Solution Files

1. 將 6 個新 `.csproj` 加入 `SystemMain_WebApi.slnx`：
   ```bash
   dotnet sln SystemMain_WebApi.slnx add Services/{{ServiceName}}/{{ServiceName}}.Cmd.API/{{ServiceName}}.Cmd.API.csproj
   dotnet sln SystemMain_WebApi.slnx add Services/{{ServiceName}}/{{ServiceName}}.Cmd.Domain/{{ServiceName}}.Cmd.Domain.csproj
   dotnet sln SystemMain_WebApi.slnx add Services/{{ServiceName}}/{{ServiceName}}.Cmd.Infrastructure/{{ServiceName}}.Cmd.Infrastructure.csproj
   dotnet sln SystemMain_WebApi.slnx add Services/{{ServiceName}}/{{ServiceName}}.Query.API/{{ServiceName}}.Query.API.csproj
   dotnet sln SystemMain_WebApi.slnx add Services/{{ServiceName}}/{{ServiceName}}.Query.Domain/{{ServiceName}}.Query.Domain.csproj
   dotnet sln SystemMain_WebApi.slnx add Services/{{ServiceName}}/{{ServiceName}}.Query.Infrastructure/{{ServiceName}}.Query.Infrastructure.csproj
   ```

2. 將測試專案加入 `Tests/Tests.slnx`：
   ```bash
   dotnet sln Tests/Tests.slnx add Tests/{{ServiceName}}.Test/{{ServiceName}}.Tests.csproj
   ```

## Step 5: 驗證建置

```bash
dotnet build SystemMain_WebApi.slnx
dotnet test Tests/Tests.slnx --filter "FullyQualifiedName~{{ServiceName}}"
```

## Step 6: 提示後續動作

建置成功後，提示使用者：

1. **Entity 實體**：在 `DBContexts/SystemMain/Models/` 下建立 `{{EntityName}}.cs` 實體類別
2. **DbSet**：在 `MainDBConnectionManager` 中加入 `DbSet<{{EntityName}}> {{EntityName}}s`
3. **EF Migration**：執行 `dotnet ef migrations add Add{{EntityName}}`
4. **GateWay 路由**：在 `GateWay/ocelot.Dev.json` 加入新服務的路由設定
5. **Docker Compose**：在 `docker-compose.yml` 加入兩個新 API 服務

---

## ⚠️ 重要提醒

- 產生的程式碼為骨架，**需依業務需求調整 Entity 屬性、DTO 欄位、EventHandler 邏輯**
- EventHandler 中的 DB 操作邏輯（欄位 mapping）需人工確認後補齊
- 若 Entity 已存在於 DBContexts，跳過建立步驟直接使用
