using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Service.Common.Logging
{
    /// <summary>
    /// 設定Serilog
    /// 在主專案的Program中執行UseSerilogSetting
    /// </summary>
    public static class SerilogSettingExtensions
    {
        public static void UseSerilogSetting(this WebApplicationBuilder builder)
        {
            try
            {
                Log.Information("Starting web host");
                //controller可以使用ILogger介面來寫入log紀錄
                builder.Host.UseSerilog((context, services, configuration) => configuration
                            .ReadFrom.Configuration(context.Configuration) // 從appsettings.json設定檔中讀取
                            .ReadFrom.Services(services)
                            .Enrich.FromLogContext());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

