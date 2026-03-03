---
applyTo: "Services/**/*.cs"
description: "CQRS + Event Sourcing patterns for microservices"
---

# CQRS 微服務架構模式

## 服務分層職責

### Cmd（Command / 寫入）側

| 層 | 職責 | 不應包含 |
|----|------|----------|
| `*.Cmd.API` | Controller 接收請求，組裝 Command，派發至 Dispatcher | 業務邏輯 |
| `*.Cmd.Domain` | Aggregate、Events、DTOs、CommandHandler、Interface | DB 存取 |
| `*.Cmd.Infrastructure` | EventHandler（將 Domain Event 寫入讀寫分離的 DB） | HTTP 邏輯 |

### Query（讀取）側

| 層 | 職責 | 不應包含 |
|----|------|----------|
| `*.Query.API` | Controller 接收請求，派發 Query 至 QueryDispatcher | 業務邏輯 |
| `*.Query.Domain` | Queries 定義 | — |
| `*.Query.Infrastructure` | QueryHandler，直接查詢讀寫分離 DB | 寫入操作 |

## Command 實作模式

```csharp
// 1. Command（繼承 BaseCommand）
public class AddControllerCommand : BaseCommand
{
    public AddControllerDTO input { get; set; }
}

// 2. Command Handler Interface
public interface ICommandHandler
{
    Task<TResult> HandleAsync(AddControllerCommand command);
    Task<TResult> HandleAsync(UpdateControllerCommand command);
}

// 3. Command Handler 實作
public async Task<TResult> HandleAsync(AddControllerCommand command)
{
    var aggregate = new ControllerAggregate(_httpContextAccessor, command.id, command.input);
    return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
}
```

## Controller 實作模式

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]          // 必須指定授權角色
public class Controller_CmdController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public Controller_CmdController(ICommandDispatcher commandDispatcher)
        => _commandDispatcher = commandDispatcher;

    [HttpPost("Add")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult> Add([FromBody] AddControllerDTO input)
    {
        var command = new AddControllerCommand
        {
            id = ConverterHelper.StringToGuid(input.someId),
            input = input,
            createOn = DateTime.UtcNow,
        };
        await _commandDispatcher.SendAsync(command);
        return Ok(new ResponseDTO { isSuccess = true, message = "Success" });
    }
}
```

## Program.cs 服務注入模式

每個 Cmd API 的 `Program.cs` 必須包含以下注入順序：

```csharp
// 1. DbContext
ServicesInjectionHelper.InjectDbContext(builder, null, serviceName, typeof(MainDBConnectionManager));

// 2. Serilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

// 3. JWT & API Security
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

// 4. CQRS 相關服務（Scoped）
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventHandler, YourService.Cmd.Infrastructure.Handlers.EventHandler>();
builder.Services.AddScoped<IEventStore, EventStore<IEventHandler>>();
builder.Services.AddScoped<IEventSourcingHandler<YourAggregate>, EventSourcingHandler>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();

// 5. Dispatcher
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

// 6. 每個 Command 的 Func 委派（用於 Dispatcher reflection-free 分發）
builder.Services.AddScoped<Func<AddXxxCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
```

> **重要**：新增 Command 時，必須同步在 Program.cs 中註冊對應的 `Func<TCommand, Task<TResult>>`。

## Aggregate 模式

```csharp
public class ControllerAggregate : AggregateRoot
{
    public ControllerAggregate() { }  // 必須有無參建構子供 Event Sourcing 重建

    public ControllerAggregate(IHttpContextAccessor httpContext, Guid id, AddControllerDTO input)
    {
        // 使用 RaiseEvent 觸發 Domain Event，不直接修改狀態
        RaiseEvent(new AddControllerEvent { /* ... */ });
    }

    public void Apply(AddControllerEvent @event)
    {
        // 在此更新 Aggregate 狀態
    }
}
```

## EventHandler（Infrastructure 層）模式

```csharp
public class EventHandler : IEventHandler
{
    private readonly IRepository<MainDBConnectionManager> _repository;

    public EventHandler(IRepository<MainDBConnectionManager> repository)
        => _repository = repository;

    public async Task<TResult> On(AddControllerEvent @event)
    {
        // 將 Event 持久化到讀寫分離的 DB
        return await _repository.CreateAsync(entity, unitOfWork);
    }
}
```

## 路由慣例

| HTTP 動詞 | 動作 | 路由範例 |
|-----------|------|----------|
| POST | 新增資源 | `POST api/v1/Controller_Cmd/Add` |
| PUT | 更新資源 | `PUT api/v1/Controller_Cmd/Update` |
| PUT | 軟刪除/停用 | `PUT api/v1/Controller_Cmd/Disable` |
| GET | 查詢列表 | `GET api/v1/Controller_Query/GetList` |
| GET | 查詢單筆 | `GET api/v1/Controller_Query/Get/{id}` |
| DELETE | 硬刪除（少用） | `DELETE api/v1/Controller_Cmd/Delete` |

## 完成修改後的自我檢查規則

**每次完成程式碼修改後，必須立即執行以下步驟，不需使用者提醒：**

1. 使用 `get_errors` 工具檢查所有被修改過的檔案（及受影響的相關檔案）是否有編譯錯誤
2. 若有錯誤，立即分析原因並修正，不得留給使用者處理
3. 修正後再次呼叫 `get_errors` 確認錯誤已消除
4. 確認無誤後才算完成任務

常見需要特別注意的錯誤類型：
- Entity 欄位型別（string / Guid / int）— 修改 Query 條件時務必先查閱 Entity 定義
- Nullable 回傳值未處理（`IEnumerable<T>?` 需先判斷 null 再呼叫 `.Any()`）
- 新增 Query/Interface 方法簽章後，所有 `IQueryHandler` 實作類別必須同步新增對應方法
- `Program.cs` 需同步注冊新增的 Handler
