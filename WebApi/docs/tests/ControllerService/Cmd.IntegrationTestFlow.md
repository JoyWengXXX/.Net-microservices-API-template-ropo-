# ControllerService Cmd API — 整合測試流程圖

## 測試情境總覽

| 測試名稱 | HTTP Method | 路由 | 身份 | 預期結果 |
|---|---|---|---|---|
| Add_AuthenticatedAdmin_Returns201Created | POST | /api/v1/Controller_Cmd/Add | Admin | 201 Created |
| Add_UnauthenticatedRequest_Returns401Unauthorized | POST | /api/v1/Controller_Cmd/Add | 匿名 | 401 |
| Update_AuthenticatedAdmin_Returns200Ok | PUT | /api/v1/Controller_Cmd/Update | Admin | 200 OK |
| Update_UnauthenticatedRequest_Returns401Unauthorized | PUT | /api/v1/Controller_Cmd/Update | 匿名 | 401 |
| Disable_AuthenticatedAdmin_Returns200Ok | DELETE | /api/v1/Controller_Cmd/{id} | Admin | 200 OK |
| Disable_UnauthenticatedRequest_Returns401Unauthorized | DELETE | /api/v1/Controller_Cmd/{id} | 匿名 | 401 |

---

## 整合測試架構

```mermaid
graph TD
    Factory[CustomWebApplicationFactory]
    Factory -->|替換| MockDispatcher[Mock ICommandDispatcher]
    Factory -->|替換| TestAuth[TestAuthHandler]
    Factory -->|建立| AdminClient[HttpClient\nX-Test-Auth: true]
    Factory -->|建立| AnonClient[HttpClient\n無 auth header]
```

---

## API 整合測試流程

### POST /Add — 已驗證 Admin

```mermaid
sequenceDiagram
    participant Test as Integration Test
    participant Client as HttpClient (Admin)
    participant MW as AuthorizationHandler Middleware
    participant Auth as TestAuthHandler
    participant Ctrl as Controller_CmdController
    participant Disp as Mock ICommandDispatcher

    Test->>Client: POST /api/v1/Controller_Cmd/Add\n{ controllerId, controllerName }
    Client->>MW: Request (X-Test-Auth: true, 無 Bearer token)
    MW->>MW: Token 為空 → 直接 next()
    MW->>Auth: UseAuthentication() 呼叫 TestAuthHandler
    Auth->>Auth: 檢查 X-Test-Auth header → 存在
    Auth-->>MW: AuthenticateResult.Success (Role=Admin)
    MW->>Ctrl: [Authorize(Roles="Admin")] 通過
    Ctrl->>Disp: SendAsync(AddControllerCommand)
    Disp-->>Ctrl: TResult { isSuccess=true }
    Ctrl-->>Test: 201 Created { isSuccess=true }
```

### POST /Add — 未驗證 (匿名)

```mermaid
sequenceDiagram
    participant Test as Integration Test
    participant Client as HttpClient (匿名)
    participant MW as AuthorizationHandler Middleware
    participant Auth as TestAuthHandler
    participant AuthZ as AuthorizationMiddleware

    Test->>Client: POST /api/v1/Controller_Cmd/Add
    Client->>MW: Request (無任何 auth header)
    MW->>MW: Token 為空 → 直接 next()
    MW->>Auth: UseAuthentication() 呼叫 TestAuthHandler
    Auth->>Auth: 無 X-Test-Auth header → Fail
    Auth-->>AuthZ: AuthenticateResult.Fail
    AuthZ-->>Test: 401 Unauthorized
```

### PUT /Update — 已驗證 Admin

```mermaid
sequenceDiagram
    participant Test as Integration Test
    participant Client as HttpClient (Admin)
    participant Ctrl as Controller_CmdController
    participant Disp as Mock ICommandDispatcher

    Test->>Client: PUT /api/v1/Controller_Cmd/Update\n{ controllerId, controllerName }
    Client->>Ctrl: 通過 TestAuthHandler & [Authorize]
    Ctrl->>Disp: SendAsync(UpdateControllerCommand)
    Disp-->>Ctrl: TResult { isSuccess=true }
    Ctrl-->>Test: 200 OK { isSuccess=true }
```

### DELETE /{controllerId} — 已驗證 Admin

```mermaid
sequenceDiagram
    participant Test as Integration Test
    participant Client as HttpClient (Admin)
    participant Ctrl as Controller_CmdController
    participant Disp as Mock ICommandDispatcher

    Test->>Client: DELETE /api/v1/Controller_Cmd/CTRL_001
    Client->>Ctrl: 通過 TestAuthHandler & [Authorize]
    Ctrl->>Disp: SendAsync(DisableControllerCommand { ControllerId="CTRL_001" })
    Disp-->>Ctrl: TResult { isSuccess=true }
    Ctrl-->>Test: 200 OK { isSuccess=true }
```

---

## 測試基礎設施說明

| 元件 | 說明 |
|---|---|
| `CustomWebApplicationFactory` | 繼承 `WebApplicationFactory<Program>`，替換 `ICommandDispatcher` 與 JWT auth |
| `TestAuthHandler` | 自訂 `AuthenticationHandler`：有 `X-Test-Auth: true` header → Admin 身份，否則 401 |
| `Mock<ICommandDispatcher>` | 暴露於 `factory.MockDispatcher`，可在每個測試中設定 Setup / Verify |
| `AdminClient` | 預設帶有 `X-Test-Auth: true` header 的 `HttpClient` |
| `AnonymousClient` | 無任何 auth header 的 `HttpClient`，用於測試 401 情境 |
