# ControllerService Query API — 重構報告

**日期**：2026-03-03  
**範圍**：`ControllerService.Query.API` / `ControllerService.Query.Infrastructure` / `ControllerService.Query.Domain`

---

## 問題清單與修正說明

### 🔴 #1 — IMapper 在建構子中手動建立（嚴重）

**問題**  
`Controller_QueryController` 每次實例化都在建構子中執行 `new MapperConfiguration(...)` 與 `config.CreateMapper()`，違反 DI 原則，且每個 Request 都重複建立 MapperConfiguration 有顯著效能開銷。

**修正**  
- `Program.cs` 新增 `builder.Services.AddAutoMapper(typeof(ControllerProfile));`  
- 建構子改為兩個參數 `(IQueryDispatcher pageDispatcher, IMapper mapper)`，由 DI 容器注入 `IMapper`  
- 移除 `using ControllerService.Query.Domain.Mappers;`（建構子不再需要）

---

### 🔴 #2 — 單筆查詢拉取全部資料再過濾（嚴重）

**問題**  
`Controller(string controllerId)` 呼叫 `GetControllersQuery`（撈全部），再於記憶體用 `.First()` 篩選，等同 N+1 查詢設計，在資料量大時效能極差。

**修正**  
新增以下三個檔案/修改：

| 檔案 | 異動 |
|------|------|
| `ControllerService.Query.Domain/Queries/GetControllerByIdQuery.cs` | 新增，含 `Guid ControllerId` 屬性 |
| `ControllerService.Query.Domain/Queries/Interfaces/IQueryHandler.cs` | 新增 `HandleAsync(GetControllerByIdQuery)` 方法簽章 |
| `ControllerService.Query.Infrastructure/Handlers/QueryHandler.cs` | 實作 `HandleAsync(GetControllerByIdQuery)`，直接用 `GetFirstAsync` + WHERE 條件查單筆 |
| `ControllerService.Query.API/Program.cs` | 新增 `pageDispatcher.RegisterHandler<GetControllerByIdQuery>(queryHandler.HandleAsync);` |

---

### 🔴 #3 — Guid vs string 型別不符導致永遠不相等（執行期錯誤）

**問題**  
原始 `Controller(string controllerId)` 將 `string` 與 `Controller.ControllerId`（`Guid`）比較，`x.ControllerId == controllerId` 永遠為 `false`，`First()` 必拋 `InvalidOperationException`。

**修正**  
`GetController` 方法參數改為 `Guid controllerId`，搭配 #2 的 `GetControllerByIdQuery` 在 DB 層直接正確比對。

---

### 🔴 #4 — Mapper 傳入 TResult 物件而非 executionData（執行期錯誤）

**問題**  
```csharp
// ❌ 原始：傳入 TResult，無對應 Profile，AutoMapper 執行期拋例外
result = _mapper.Map<List<GetControllerDTO>>(result)
```
`ControllerProfile` 僅設定 `Controller` → `GetControllerDTO`，並無 `TResult` → `List<GetControllerDTO>` 的映射。

**修正**  
```csharp
// ✅ 修正：先取出 executionData 並轉型後再 Map
result = _mapper.Map<List<GetControllerDTO>>((List<SystemMain.Entities.Controller>)result.executionData)
```

---

### 🟠 #5 — 缺少錯誤狀態碼的 ProducesResponseType（中）

**問題**  
兩個 Action 只宣告 `Status200OK`，Swagger UI 看不到認證失敗與 Not Found 的回應格式。

**修正**  
兩個 Action 均補上：
```csharp
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
```

---

### 🟡 #6 — ActionResult 未使用泛型版本（低）

**問題**  
`Task<ActionResult>` 不攜帶型別資訊，OpenAPI Schema 無法推導回應結構。

**修正**  
兩個 Action 均改為 `Task<ActionResult<ResponseDTO>>`。

---

### 🟡 #7 — 方法名稱 `Controller` 與 ControllerBase 屬性語義衝突（低）

**問題**  
`ControllerBase` 有 `Controller` context 屬性，同名方法雖可編譯，但可讀性差，易造成混淆。

**修正**  
方法改名為 `GetController`。

---

### 🟡 #8 — Query String 改為 Route Parameter（低）

**問題**  
`/api/v1/Controller_Query/Controller?controllerId=xxx` 不符合 REST 慣例，單筆資源應以 ID 作為路由參數。

**修正**  
```csharp
// ❌ 舊
[HttpGet("Controller")]
public async Task<ActionResult> Controller(string controllerId)

// ✅ 新
[HttpGet("Controller/{controllerId:guid}")]
public async Task<ActionResult<ResponseDTO>> GetController(Guid controllerId)
```
加入 `:guid` 路由限制，讓非 GUID 格式的請求在路由層直接回傳 `404`，不進 Action。

---

## 異動檔案彙整

| 檔案 | 異動類型 |
|------|---------|
| `Services/ControllerService/ControllerService.Query.Domain/Queries/GetControllerByIdQuery.cs` | 新增 |
| `Services/ControllerService/ControllerService.Query.Domain/Queries/Interfaces/IQueryHandler.cs` | 修改 |
| `Services/ControllerService/ControllerService.Query.Infrastructure/Handlers/QueryHandler.cs` | 修改 |
| `Services/ControllerService/ControllerService.Query.API/Program.cs` | 修改 |
| `Services/ControllerService/ControllerService.Query.API/Controllers/Controller_QueryController.cs` | 修改 |
