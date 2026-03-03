using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ControllerService.Tests.Infrastructure;

/// <summary>
/// 整合測試用的假 Authentication Handler。
/// Header "X-Test-Auth" 的值即為角色名稱，例如 "Admin" 或 "User"。
/// 不帶 Header 則視為未驗證（401）。
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public const string TestAuthHeader = "X-Test-Auth";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TestAuthHeader, out var roleValues))
            return Task.FromResult(AuthenticateResult.Fail("Missing test auth header"));

        var role = roleValues.ToString();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, $"Test{role}User"),
            new Claim(ClaimTypes.Role, role),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
