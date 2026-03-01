using CommonLibrary.Helpers;
using CQRS.Core.DefaultConcreteObjects.Config;
using CQRS.Core.DefaultConcreteObjects.Repository;
using CQRS.Core.Domain;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Service.Common.Authorization;
using Service.Common.Authorization.Interfaces;
using Service.Common.Filters;
using Service.Common.Models.DTOs;
using StackExchange.Redis;
using System.Data;
using System.Reflection;

namespace Service.Common.Helpers
{
    public static class ServicesInjectionHelper
    {
        public static void InjectDbContext(this IHostApplicationBuilder builder, string? path = null, string serviceName = "", params Type[] dbContextTypes)
        {
            InjectDbContextInternal(builder.Services, path, serviceName, dbContextTypes);
        }

        public static void InjectDbContext(this WebApplicationBuilder builder, string? path = null, string serviceName = "", params Type[] dbContextTypes)
        {
            InjectDbContextInternal(builder.Services, path, serviceName, dbContextTypes);
        }

        public static void InitializeApiServicesAndSecurity(this WebApplicationBuilder builder)
        {
            //JWT設定
            builder.JWTSetting();

            builder.Services.AddSwaggerGen(options =>
            {
                //設定Swagger產生API文件
                //using System.Reflection;
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                //設定API版本
                options.SwaggerDoc("v1", new OpenApiInfo { Title = $"SystemMain", Version = $"v_{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")}" });

                //設定Bearer授權
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT授權描述"
                });
                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            builder.Services.AddScoped<IJwtHelper, JwtHelper>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .Select(e => new
                        {
                            Field = e.Key,
                            Errors = e.Value.Errors.Select(err => err.ErrorMessage).ToArray()
                        })
                        .ToArray();

                    var result = new ResponseDTO
                    {
                        result = null,
                        isSuccess = false,
                        message = "Required field is not filled!"
                    };

                    return new BadRequestObjectResult(result);
                };
            });
        }

        public static void InitialzeSerilogSettings(this WebApplicationBuilder builder, string serviceName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var commonConfigs = ConfigurationHelper.LoadConfiguration(
                directoryInfo.FullName,
                "CommonSettings"
            );

            // GCP VM 的 IP 或域名
            var seqServerUrl = commonConfigs.GetValue<string>("Seq:ServerUrl");
            var seqApiKey = commonConfigs.GetValue<string>("Seq:ApiKey");

            // 根據環境決定日誌級別
            var minimumLevel = builder.Environment.IsProduction() 
                ? LogEventLevel.Information  // 生產環境使用 Information
                : LogEventLevel.Debug;       // 開發環境使用 Debug

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)  // 減少 EF Core 日誌
                .MinimumLevel.Override("System", LogEventLevel.Warning)  // 減少系統日誌
                .WriteTo.Console()
                .WriteTo.File(
                    path: $"Logs/{serviceName}/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 90,
                    fileSizeLimitBytes: 50000000) // 50MB
                .WriteTo.Seq(
                    serverUrl: seqServerUrl,
                    apiKey: seqApiKey,
                    restrictedToMinimumLevel: minimumLevel,  // 只發送達到最低級別的日誌到 Seq
                    batchPostingLimit: 100,  // 批次發送，減少網路開銷
                    period: TimeSpan.FromSeconds(2))  // 每 2 秒發送一次
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)  // 區分環境
                .Enrich.WithProperty("MachineName", Environment.MachineName)  // 添加機器名稱
                .CreateLogger();

            Directory.CreateDirectory($"Logs/{serviceName}");

            builder.Host.UseSerilog();
        }

        private static void InjectDbContextInternal(IServiceCollection services, string? path = null, string serviceName = "", params Type[] dbContextTypes)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string configPath = path ?? directoryInfo.FullName;

            var dbContextSettings = DBContextSettingsHelper.GetSettings();

            // PostgreSQL
            foreach (var dbContextType in dbContextTypes)
            {
                if (typeof(IProjectDBConnectionManager).IsAssignableFrom(dbContextType))
                {
                    string connectionString = string.Empty;
                    if (dbContextType.Name == typeof(MainDBConnectionManager).Name)
                        connectionString = dbContextSettings.connectionStrings.DefaultConnection;

                    services.AddScoped(dbContextType, sp =>
                    {
                        return ActivatorUtilities.CreateInstance(sp, dbContextType, connectionString);
                    });
                }
            }
            services.AddSingleton<DatabaseFactory>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Event store (PostgreSQL)
            var eventStoreConfig = dbContextSettings.eventStorePostgresConfig ?? new EventStorePostgresConfig();

            services.Configure<EventStorePostgresConfig>(options =>
            {
                options.ConnectionString = string.IsNullOrWhiteSpace(eventStoreConfig.ConnectionString)
                    ? dbContextSettings.connectionStrings.DefaultConnection
                    : eventStoreConfig.ConnectionString;
                options.Schema = "public";
                options.Table = "EventSourcingEvent";
            });

            services.AddSingleton<IEventStoreConnectionManager, EventStorePostgresConnectionManager>();
            services.AddScoped<IEventStoreRepository, EventStoreRepository>();

            // Redis configuration
            var redisConfig = new ConfigurationOptions
            {
                EndPoints = { dbContextSettings.redisWithAOFConnectionConfig.ConnectionString },
                AbortOnConnectFail = false,
                ConnectRetry = 5,
                ConnectTimeout = 50000,
                SyncTimeout = 50000,
                AllowAdmin = true,
                DefaultDatabase = 0,
                KeepAlive = 60,
                ResolveDns = true
            };
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var multiplexer = ConnectionMultiplexer.Connect(redisConfig);
                return multiplexer;
            });
        }

        /// <summary>
        /// 為 Console 應用程式添加 JWT 設定
        /// </summary>
        /// <param name="builder">主機建構器</param>
        public static void ConfigureJwtServices(this IHostApplicationBuilder builder)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var configuration = ConfigurationHelper.LoadConfiguration(
                directoryInfo.FullName,
                "CommonSettings"
            );

            builder.Services.Configure<JwtSettingsOptions>(configuration.GetSection("JWTSettings"));
            builder.Services.AddScoped<IJwtHelper, JwtHelper>();
            builder.Services.AddHttpContextAccessor();
        }
    }
}



