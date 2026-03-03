---
description: "Generate unit tests for a given Handler or Service class following TDD patterns"
name: "Generate Unit Tests"
argument-hint: "Specify the handler or class to test (e.g., 'EventHandler for ControllerService')"
agent: "tdd-developer"
tools: [read, search, edit, execute]
---

# Generate Unit Tests

Generate comprehensive unit tests for the specified handler or class.

**Target**: {{handler or class name / service name}}

## 測試目標層級

> ⛔ 本 prompt 的**首要目標**是生成 `EventHandler` 與 `QueryHandler` 的**單元測試**。
> 這兩層才是 CQRS 架構中商務邏輯的真正所在，必須優先覆蓋。

> ⛔ **嚴格禁止**：只 Mock `ICommandDispatcher` 然後斷言 Controller 回傳 200/201 — 這等同於測試框架本身，無任何商務邏輯覆蓋。

> ✅ Controller 整合測試**唯一有價值**的情境：`401 Unauthorized`、`403 Forbidden`、`400 Bad Request（Model Validation）`。
> 這些情境驗證的是 Auth middleware 與 ASP.NET Model Binding，而非 Mock 成功路徑。
> Controller 整合測試請使用 `/integration-test-flow` prompt 生成。

## What to Generate

1. **Read the target class** (`EventHandler.cs` or `QueryHandler.cs`) to understand all public methods
2. **Read the spec** from `docs/specs/` if available — use Test Scenarios table (section 8) as the test case list
3. **Generate test file** following the project test conventions

## EventHandlerTests.cs 必要情境

針對 EventHandler 的每個 `On(XxxEvent)` 方法，生成：

- [ ] **Happy path** — 正常成功回傳 (`isSuccess = true`)
- [ ] **Not found** — `GetFirstAsync` 回傳 `null` → `ThrowsAsync<AppException>`（Update / Disable 必備）
- [ ] **Verify** — 確認 DB 方法（`CreateAsync` / `UpdateAsync` / `DeleteAsync`）被呼叫 `Times.Once`
- [ ] **UnitOfWork mock** — 凡有 `CreateUnitOfWork()` 的方法，建構子必須 `_mockRepo.Setup(x => x.CreateUnitOfWork()).Returns(_mockUoW.Object)`

## QueryHandlerTests.cs 必要情境

針對 QueryHandler 的每個 `HandleAsync(XxxQuery)` 方法，生成：

- [ ] **Happy path** — 回傳正確資料集
- [ ] **Empty / Not found** — `GetListAsync` 回傳空集合或 null → `ThrowsAsync<AppException>`
- [ ] **Verify** — 確認 `GetListAsync` 或 `GetFirstAsync` 被呼叫 `Times.Once`

## File Placement

- Test file location: `Tests/{ServiceName}.Test/{HandlerType}Tests.cs`
- If test project doesn't exist, create it with proper `.csproj` and `GlobalUsings.cs`
- Add to `Tests/Tests.slnx` if project is new

## After Generation

Run the tests to confirm:
```bash
dotnet test Tests/Tests.slnx --filter "FullyQualifiedName~{TestClassName}"
```

Report a summary table:
| Test Method | Scenario | Pass/Fail |
|-------------|----------|-----------|
