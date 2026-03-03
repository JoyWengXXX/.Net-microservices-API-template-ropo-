---
applyTo: "Tests/**/*.cs"
description: "TDD and unit testing standards for xUnit + Moq"
---

# TDD 與測試規範

## 核心原則

1. **每個新功能或修改** 必須附帶對應的單元測試
2. 測試驅動：先寫測試，讓測試失敗（Red），再實作讓其通過（Green），最後重構（Refactor）
3. 測試是功能的活文件，命名應清晰描述「情境 → 預期結果」

## 測試分層原則（重要）

本專案採用 CQRS 架構，**各層的測試職責明確分開**：

| 層次 | 有商務邏輯？ | 測試方式 | 測試目標 |
|------|------------|----------|----------|
| Controller | ❌ 只負責參數封裝與 Dispatch | 整合測試 | HTTP 狀態碼、路由、Auth |
| CommandHandler | ✅ Aggregate 建立與 EventSourcing 協調 | 單元測試 | 正確 Event 被 Raise |
| EventHandler (Infra) | ✅ 資料庫 CRUD、Not-Found 檢查、Transaction | **單元測試（首要目標）** | DB 操作、例外、Rollback |
| QueryHandler (Infra) | ✅ 資料查詢、結果驗證 | **單元測試（首要目標）** | 查詢結果、Not-Found 例外 |

> ⚠️ **禁止**：只 Mock ICommandDispatcher 然後測試 Controller 回傳的 HTTP 狀態碼，這等同於測試框架本身，完全沒有覆蓋到商務邏輯。

## 測試命名慣例

格式：`{Method}_{Scenario}_{ExpectedResult}`

```csharp
// ✅ 正確範例
public async Task On_AddControllerEvent_ReturnsTResultWithSuccess()
public async Task HandleAsync_WhenAggregateNotFound_ThrowsAggregateNotFoundException()
public async Task GetControllersQuery_WithMultipleControllers_ReturnsAllEnabledItems()

// ❌ 錯誤範例
public async Task TestAdd()
public async Task Test1()
```

## 測試結構：Arrange / Act / Assert

```csharp
[Fact]
public async Task On_AddControllerEvent_ReturnsTResultWithSuccess()
{
    // Arrange
    var @event = new AddControllerEvent
    {
        controllerName = "testController",
    };
    mockRepo.Setup(x => x.CreateAsync(
        It.IsAny<Controller>(),
        It.IsAny<IUnitOfWork>()))
        .ReturnsAsync(1);

    // Act
    var result = await _eventHandler.On(@event);

    // Assert
    Assert.True(result.isSuccess);
}
```

## 測試類別結構

```csharp
public class EventHandlerTests
{
    // Mock 成員變數（readonly）
    private readonly Mock<IRepository<MainDBConnectionManager>> _mockRepo;
    private readonly EventHandler _sut;  // System Under Test

    // 建構子只做 SUT 初始化，不放業務邏輯
    public EventHandlerTests()
    {
        _mockRepo = new Mock<IRepository<MainDBConnectionManager>>();
        _sut = new EventHandler(_mockRepo.Object);
    }

    // 每個 [Fact] 覆蓋一個具體情境
    // 相關測試用 [Theory] + [InlineData] 組合
}
```

## Mock 慣例

```csharp
// Setup：明確指定預期參數（能具體就不用 It.IsAny）
_mockRepo.Setup(x => x.GetFirstAsync(
    It.IsAny<Expression<Func<Controller, object>>>(),
    It.IsAny<Expression<Func<Controller, bool>>>(),
    null,
    It.IsAny<IUnitOfWork>()))
    .ReturnsAsync(mockedControllerObject);

// Verify：驗證關鍵互動確實發生
_mockRepo.Verify(x => x.CreateAsync(
    It.Is<Controller>(c => c.ControllerName == "testController"),
    It.IsAny<IUnitOfWork>()),
    Times.Once);
```

## 使用 UnitOfWork 的 EventHandler 測試設定

當 EventHandler 的方法內呼叫 `_repo.CreateUnitOfWork()`（Update / Disable 等需要 Transaction 的操作），
**必須 Mock `CreateUnitOfWork()`**，否則測試中會 NullReferenceException：

```csharp
public class EventHandlerTests
{
    private readonly Mock<IRepository<MainDBConnectionManager>> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUoW;
    private readonly EventHandler _sut;

    public EventHandlerTests()
    {
        _mockRepo = new Mock<IRepository<MainDBConnectionManager>>();
        _mockUoW  = new Mock<IUnitOfWork>();
        _mockRepo.Setup(x => x.CreateUnitOfWork()).Returns(_mockUoW.Object);
        _sut = new EventHandler(_mockRepo.Object);
    }
}
```

## 必覆蓋情境清單

EventHandler / QueryHandler 每個方法至少覆蓋：

| 情境 | 測試類型 |
|------|----------|
| 正常流程 → isSuccess=true | [Fact] |
| 資源不存在（GetFirstAsync 回 null）→ ThrowsAsync`<AppException>` | [Fact] |
| Verify：關鍵 DB 方法被呼叫 Times.Once | [Fact] |
| 邊界值（null、空字串）| [Theory] + [InlineData] |

## 測試專案設定

- 每個 Service 對應一個測試專案，位於 `Tests/{ServiceName}.Test/`
- 測試專案命名：`{ServiceName}.Tests`（複數）
- Implicit usings 統一定義在 `GlobalUsings.cs`：
  ```csharp
  global using Xunit;
  global using Moq;
  global using DataAccess.Interfaces;
  global using DataAccess;
  global using DBContexts.SystemMain;
  global using DBContexts.SystemMain.Models;
  ```
- 使用 `Tests.slnx` 管理所有測試專案

## 整合測試

- 整合測試流程以 Mermaid sequence diagram 記錄於 `docs/tests/{ServiceName}/` 子目錄
- 每個服務的測試文件集中放在對應子目錄，例如 `docs/tests/ControllerService/Cmd.IntegrationTestFlow.md`
- 每個跨服務流程都應有對應的流程圖
- 使用 `/integration-test-flow` prompt 自動產生
