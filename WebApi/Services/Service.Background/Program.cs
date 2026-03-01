using DataAccess;
using RabbitMQ.Client;
using Service.Background.Handlers;
using Service.Background.Services;
using Service.Background.Services.Interfaces;
using Service.Background.Models;
using Microsoft.Extensions.Options;
using System.Reflection;
using Service.Common.Helpers;
using Service.Common.Helpers.Interfaces;
using CommonFilesManagement.Interfaces;
using CommonFilesManagement;
using SystemMain;
using Services.Shared.MachineAccounting.Services.Interfaces;
using Services.Shared.MachineAccounting.Services;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// æ³¨å…¥DbContext
ServicesInjectionHelper.InjectDbContext(builder, null, "", typeof(MainDBConnectionManager), typeof(MetersDBConnectionManager));

// è¨­ç½® JWT ?¸é??å?
ServicesInjectionHelper.ConfigureJwtServices(builder);

// è¨­ç½® RabbitMQ ???
builder.Services.AddSingleton(sp =>
{
    var DBconfig = DBContextSettingsHelper.GetSettings();
    var factory = new ConnectionFactory
    {
        HostName = DBconfig.rabbitMQServer.Host,
        Port = DBconfig.rabbitMQServer.Port,
        UserName = DBconfig.rabbitMQServer.Username,
        Password = DBconfig.rabbitMQServer.Password,
        VirtualHost = DBconfig.rabbitMQServer.VirtualHost,
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        RequestedHeartbeat = TimeSpan.FromSeconds(30)
    };
    return factory.CreateConnection();
});

// è¨­ç½® Redis ???
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var DBconfig = DBContextSettingsHelper.GetSettings();
    var redisConfig = ConfigurationOptions.Parse(DBconfig.redisWithAOFConnectionConfig.ConnectionString);
    redisConfig.AbortOnConnectFail = false;
    redisConfig.ConnectRetry = 3;
    redisConfig.ConnectTimeout = 5000;
    return ConnectionMultiplexer.Connect(redisConfig);
});

// æ³¨å…¥ Service
builder.Services.AddScoped<IControllerInputAndOutputHelper, ControllerInputAndOutputHelper>();
builder.Services.AddScoped<IPostgresChangeDetector, PostgresChangeDetector>();
builder.Services.AddScoped<IHttpClientRequestHelper, HttpClientRequestHelper>();
builder.Services.AddScoped<IMeterValuesArrangement, MeterValuesArrangement>();
builder.Services.AddScoped<ICommonFileManager, CommonFileManager>();
builder.Services.AddScoped<IFcmNotificationService, FcmNotificationService>();
builder.Services.AddSingleton<ITaskLockManager, TaskLockManager>(); // è¨»å?ä»»å??–å?ç®¡ç???builder.Services.AddScoped<IKeepAliveAsyncService, KeepAliveAsyncService>();
builder.Services.AddScoped<ILogInTokenClearService, LogInTokenClearService>();
builder.Services.AddScoped<IPcbDisableService, PcbDisableService>();
builder.Services.AddScoped<IPcbRegistryApplyClear, PcbRegistryApplyClearService>();
builder.Services.AddScoped<IStoreClearService, StoreClearService>();
builder.Services.AddScoped<IDailyRecordSettlementService, DailyRecordSettlementService>();
builder.Services.AddScoped<IHourlyRecordPersistenceService, HourlyRecordPersistenceService>();
builder.Services.AddScoped<IDailyAccountingAggregationService, DailyAccountingAggregationService>();
builder.Services.AddScoped<IDailyRecordsSettlement, DailyRecordsSettlement>();
builder.Services.AddScoped<IHourlyAccountingRecalculationService, HourlyAccountingRecalculationService>();
builder.Services.AddSingleton<IDbConnectionMonitorService, DbConnectionMonitorService>();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// æ³¨å…¥ Handler
builder.Services.AddScoped<LogInTokenClearTaskHandler>();
builder.Services.AddScoped<StoreClearTaskHandler>();
builder.Services.AddScoped<PcbRegistryApplyClearTaskHandler>();
builder.Services.AddScoped<KeepAliveAsyncTaskHandler>();
builder.Services.AddScoped<EgmKeepAliveAsyncTaskHandler>();
builder.Services.AddScoped<PcbDisableTaskHandler>();
builder.Services.AddScoped<DailyRecordSettlementTaskHandler>();
builder.Services.AddScoped<HourlyRecordPersistenceTaskHandler>();
builder.Services.AddScoped<DailyAccountingAggregationTaskHandler>();
builder.Services.AddScoped<DbConnectionMonitorAsyncTaskHandler>();

// æ³¨å…¥ TaskConfig
builder.Services.Configure<List<TaskConfig>>(builder.Configuration.GetSection("Tasks"));

// æ³¨å…¥ ITaskHandler ?•ç?
var handlerTypes = Assembly.GetExecutingAssembly() //?²å??‡å??½å?ç©ºé??§æ??‰Handler
    .GetTypes()
    .Where(t => t.Namespace == "Service.Background.Handlers" &&
                typeof(ITaskHandler).IsAssignableFrom(t) &&
                !t.IsInterface && !t.IsAbstract)
    .ToList();

foreach (var handlerType in handlerTypes)
{
    builder.Services.AddScoped(handlerType);
}
builder.Services.AddScoped<IEnumerable<ITaskHandler>>(sp =>
{
    var taskConfigs = sp.GetRequiredService<IOptions<List<TaskConfig>>>().Value;
    var handlers = new List<ITaskHandler>();

    foreach (var config in taskConfigs)
    {
        var handlerType = handlerTypes.FirstOrDefault(t => t.Name == $"{config.TaskName}Handler");
        if (handlerType == null)
        {
            throw new InvalidOperationException($"No handler found for task: {config.TaskName}");
        }

        var handler = (ITaskHandler)sp.GetRequiredService(handlerType);
        handler.TaskName = config.TaskName;
        handlers.Add(handler);
    }

    return handlers;
});

// æ³¨å…¥ Worker ??MQTaskScheduler
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<MQTaskScheduler>();

var host = builder.Build();
host.Run();
