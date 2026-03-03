---
applyTo: "**/*.cs"
description: "C# coding style for this .NET microservices project"
---

# C# 編碼風格規範

## 命名慣例

| 類型 | 慣例 | 範例 |
|------|------|------|
| Class / Interface / Enum | PascalCase | `CommandHandler`, `ICommandDispatcher` |
| Method | PascalCase (async 加 Async 後綴) | `HandleAsync`, `GetByIdAsync` |
| Property | PascalCase | `ControllerId`, `IsEnable` |
| Private field | `_camelCase` | `_eventSourcingHandler` |
| Local variable / Parameter | camelCase | `command`, `aggregate` |
| DTO property | camelCase（保持 JSON 序列化一致） | `controllerId`, `controllerName` |
| 常數 | PascalCase | `DefaultPageSize` |

## 命名空間 / 檔案組織

- 命名空間反映專案分層：`ServiceName.Cmd.API.Commands`
- 一個檔案一個 public 類型
- 介面檔案放在 `Interfaces/` 子資料夾

## 語法規範

```csharp
// ✅ 正確：使用 async/await
public async Task<TResult> HandleAsync(AddControllerCommand command)
{
    var aggregate = new ControllerAggregate(...);
    return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
}

// ❌ 禁止：.Result / .Wait()
var result = _handler.HandleAsync(cmd).Result; // NEVER

// ✅ 正確：using 宣告（非 using 語句）
using var context = new DbContext();

// ✅ 建構子注入
public CommandHandler(
    IEventSourcingHandler<ControllerAggregate> eventSourcingHandler,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
{
    _eventSourcingHandler = eventSourcingHandler;
    _httpContextAccessor = httpContextAccessor;
    _isEventSourcingEnabled = configuration.GetValue<bool>("EventSouringEnable");
}
```

## 例外處理

- 不在 Handler 內 catch 然後吞掉例外
- 使用 middleware 統一處理未預期例外
- 已知業務例外（如 `AggregateNotFoundException`）應讓 middleware 轉換為適當 HTTP 狀態碼

## 回傳型別

- Controller action 回傳 `ActionResult` / `ActionResult<T>`
- Command/Query Handler 回傳 `Task<TResult>`
- Repository 方法視需要回傳 `Task<int>`（影響列數）或 `Task<T>`

## 依賴注入生命週期

- **Scoped**（預設）：每個 HTTP 請求一個實例
- **Singleton**：僅用於無狀態的 utility（如 Helper），需明確標示
- **Transient**：幾乎不使用

## 格式

- 縮排：4 個空格
- 大括號：Allman 風格（開括號另起一行）
- 每個方法之間空一行
- using 陳述式依序：System → Microsoft → 第三方 → 專案內部
