# ControllerService 整合測試說明

> **日期**：2026-03-03  
> **測試檔案**：`Tests/ControllerService.Test/Controller_CmdIntegrationTests.cs`  
> **涵蓋 API**：`POST api/v1/Controller_Cmd/Add`、`PUT api/v1/Controller_Cmd/Update`、`DELETE api/v1/Controller_Cmd/{controllerId}`

---

## 測試範圍說明

本整合測試**僅驗證 Authorization 與 Model Validation 層**，不涵蓋業務邏輯。

業務邏輯（DB CRUD、例外處理、Transaction 管理）由以下單元測試負責：
- `EventHandlerTests.cs` — EventHandler 的 On 方法（新增、更新、停用）
- `QueryHandlerTests.cs` — QueryHandler 的 HandleAsync 方法（清單、單筆查詢）

---

## 測試情境索引

| # | 測試方法名稱 | 驗證目標 | HTTP 動詞 + 路由 | 預期狀態碼 |
|---|------------|---------|----------------|----------|
| 1 | `Add_UnauthenticatedRequest_Returns401Unauthorized` | 未帶 JWT token | `POST /api/v1/Controller_Cmd/Add` | 401 |
| 2 | `Add_NonAdminRole_Returns403Forbidden` | 非 Admin 角色 | `POST /api/v1/Controller_Cmd/Add` | 403 |
| 3 | `Add_MissingControllerId_Returns400BadRequest` | `controllerId` 為 `[Required]` 缺失 | `POST /api/v1/Controller_Cmd/Add` | 400 |
| 4 | `Add_MissingControllerName_Returns400BadRequest` | `controllerName` 為 `[Required]` 缺失 | `POST /api/v1/Controller_Cmd/Add` | 400 |
| 5 | `Update_UnauthenticatedRequest_Returns401Unauthorized` | 未帶 JWT token | `PUT /api/v1/Controller_Cmd/Update` | 401 |
| 6 | `Update_NonAdminRole_Returns403Forbidden` | 非 Admin 角色 | `PUT /api/v1/Controller_Cmd/Update` | 403 |
| 7 | `Update_MissingControllerId_Returns400BadRequest` | `controllerId` 為 `[Required]` 缺失 | `PUT /api/v1/Controller_Cmd/Update` | 400 |
| 8 | `Update_MissingControllerName_Returns400BadRequest` | `controllerName` 為 `[Required]` 缺失 | `PUT /api/v1/Controller_Cmd/Update` | 400 |
| 9 | `Disable_UnauthenticatedRequest_Returns401Unauthorized` | 未帶 JWT token | `DELETE /api/v1/Controller_Cmd/{controllerId}` | 401 |
| 10 | `Disable_NonAdminRole_Returns403Forbidden` | 非 Admin 角色 | `DELETE /api/v1/Controller_Cmd/{controllerId}` | 403 |

---

## 測試基礎設施說明

### CustomWebApplicationFactory

位於 `Tests/ControllerService.Test/Infrastructure/CustomWebApplicationFactory.cs`

| 替換項目 | 原始實作 | 測試替換 | 原因 |
|---------|---------|---------|------|
| Authentication | JWT Bearer（需連線 identity server） | `TestAuthHandler` | 避免測試環境依賴外部 JWT |
| `ICommandDispatcher` | 真實 `CommandDispatcher`（會觸發 DB） | `Mock<ICommandDispatcher>` | 避免測試環境依賴 DB，Auth/Validation 測試不需要業務邏輯 |

> ⚠️ `ICommandDispatcher` Mock 的用途僅是讓 Controller 不拋出例外，**不應在測試中斷言 Mock 是否被呼叫**。

### TestAuthHandler

位於 `Tests/ControllerService.Test/Infrastructure/TestAuthHandler.cs`

透過 HTTP Header `X-Test-Auth` 控制模擬身份：

| Header 值 | 模擬身份 | 適用情境 |
|-----------|---------|---------|
| `Admin` | 已驗證、角色 = Admin | 測試成功通過 Auth 的請求 |
| `User`（或任何非 Admin 字串） | 已驗證、但角色不符 | 觸發 `403 Forbidden` |
| （不帶 Header） | 未驗證 | 觸發 `401 Unauthorized` |

---

## 什麼情境未被此整合測試覆蓋（以及由誰覆蓋）

| 未覆蓋情境 | 覆蓋位置 |
|-----------|----------|
| 新增 Controller 成功後 DB 有正確資料 | `EventHandlerTests.On_AddControllerEvent_ReturnsTResultWithSuccess` |
| 更新時找不到 Controller 拋出 AppException | `EventHandlerTests.On_UpdateControllerEvent_WhenControllerNotFound_ThrowsAppException` |
| 停用時找不到 Controller 拋出 AppException | `EventHandlerTests.On_DisableControllerEvent_WhenControllerNotFound_ThrowsAppException` |
| DB 操作失敗時正確執行 Rollback | `EventHandlerTests.On_UpdateControllerEvent_WhenUpdateFails_CallsRollbackAndRethrows` |
| 查詢所有 Controller 回傳正確清單 | `QueryHandlerTests.HandleAsync_GetControllersQuery_WithData_ReturnsAllControllers` |
| 查詢空資料拋出 AppException | `QueryHandlerTests.HandleAsync_GetControllersQuery_WhenEmpty_ThrowsAppException` |
| 以 ID 查詢找不到資料拋出 AppException | `QueryHandlerTests.HandleAsync_GetControllerByIdQuery_WhenNotFound_ThrowsAppException` |
