using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControllerService.Tests.Infrastructure;

/// <summary>
/// 整合測試用的 WebApplicationFactory。
/// - 以 TestAuthHandler 替換 JWT Bearer 驗證
/// - 以可控的 Mock&lt;ICommandDispatcher&gt; 替換真實 Dispatcher
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<ICommandDispatcher> MockDispatcher { get; } = new Mock<ICommandDispatcher>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // 替換 ICommandDispatcher 為可控 Mock
            services.RemoveAll<ICommandDispatcher>();
            services.AddScoped(_ => MockDispatcher.Object);

            // 將預設驗證 Scheme 改為 TestAuthHandler（取代 JWT Bearer）
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });
    }
}
