using System.Net;
using System.Net.Http.Json;
using ControllerService.Cmd.Domain.DTOs;
using ControllerService.Tests.Infrastructure;
using Service.Common.Models.DTOs;

namespace ControllerService.Tests;

/// <summary>
/// Controller_CmdController 整合測試。
/// 測試範圍：Authorization（401/403）與 Model Validation（400）。
/// 業務邏輯（成功路徑）由 EventHandlerTests / QueryHandlerTests 覆蓋。
/// </summary>
public class Controller_CmdIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _adminClient;
    private readonly HttpClient _nonAdminClient;
    private readonly HttpClient _anonymousClient;

    public Controller_CmdIntegrationTests(CustomWebApplicationFactory factory)
    {
        // 已驗證的 Admin 用戶端
        _adminClient = factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, "Admin");

        // 已驗證但非 Admin 角色（用於測試 403）
        _nonAdminClient = factory.CreateClient();
        _nonAdminClient.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, "User");

        // 未驗證的用戶端（用於測試 401）
        _anonymousClient = factory.CreateClient();
    }

    // ── Add ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Add_UnauthenticatedRequest_Returns401Unauthorized()
    {
        var response = await _anonymousClient.PostAsJsonAsync(
            "api/v1/Controller_Cmd/Add",
            new AddControllerDTO { controllerId = "CTRL_001", controllerName = "Test" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Add_NonAdminRole_Returns403Forbidden()
    {
        var response = await _nonAdminClient.PostAsJsonAsync(
            "api/v1/Controller_Cmd/Add",
            new AddControllerDTO { controllerId = "CTRL_001", controllerName = "Test" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Add_MissingControllerId_Returns400BadRequest()
    {
        // controllerName 存在但 controllerId 缺少（[Required] 違反）
        var response = await _adminClient.PostAsJsonAsync(
            "api/v1/Controller_Cmd/Add",
            new { controllerName = "TestName" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_MissingControllerName_Returns400BadRequest()
    {
        // controllerId 存在但 controllerName 缺少（[Required] 違反）
        var response = await _adminClient.PostAsJsonAsync(
            "api/v1/Controller_Cmd/Add",
            new { controllerId = "CTRL_001" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_UnauthenticatedRequest_Returns401Unauthorized()
    {
        var response = await _anonymousClient.PutAsJsonAsync(
            "api/v1/Controller_Cmd/Update",
            new UpdateControllerDTO { controllerId = "CTRL_001", controllerName = "Updated" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_NonAdminRole_Returns403Forbidden()
    {
        var response = await _nonAdminClient.PutAsJsonAsync(
            "api/v1/Controller_Cmd/Update",
            new UpdateControllerDTO { controllerId = "CTRL_001", controllerName = "Updated" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_MissingControllerId_Returns400BadRequest()
    {
        var response = await _adminClient.PutAsJsonAsync(
            "api/v1/Controller_Cmd/Update",
            new { controllerName = "UpdatedName" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_MissingControllerName_Returns400BadRequest()
    {
        var response = await _adminClient.PutAsJsonAsync(
            "api/v1/Controller_Cmd/Update",
            new { controllerId = "CTRL_001" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // ── Disable ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Disable_UnauthenticatedRequest_Returns401Unauthorized()
    {
        var response = await _anonymousClient.DeleteAsync(
            "api/v1/Controller_Cmd/CTRL_001",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Disable_NonAdminRole_Returns403Forbidden()
    {
        var response = await _nonAdminClient.DeleteAsync(
            "api/v1/Controller_Cmd/CTRL_001",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
