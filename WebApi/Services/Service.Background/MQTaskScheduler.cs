using RabbitMQ.Client;
using System.Text;
using System.Collections.Concurrent;
using Service.Background.Services.Interfaces;
using Service.Background.Models;

public class MQTaskScheduler : BackgroundService
{
    private readonly ILogger<MQTaskScheduler> _logger;
    private readonly IConnection _connection;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private IModel _channel;
    private ConcurrentDictionary<string, DateTime> _lastRunTimes = new ConcurrentDictionary<string, DateTime>();

    public MQTaskScheduler(
        ILogger<MQTaskScheduler> logger,
        IConnection connection,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _connection = connection;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _channel = _connection.CreateModel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var taskConfigs = _configuration.GetSection("Tasks").Get<List<TaskConfig>>();
            ValidateConfigurations(taskConfigs);

            List<TaskHandlerConfig> tasks;
            using (var scope = _serviceProvider.CreateScope())
            {
                var taskHandlers = scope.ServiceProvider.GetRequiredService<IEnumerable<ITaskHandler>>();
                tasks = taskHandlers.Join(taskConfigs,
                    handler => handler.TaskName,
                    config => config.TaskName,
                    (handler, config) => new TaskHandlerConfig { Handler = handler, Config = config })
                    .ToList();
            }

            // 初始化每個任務的隊列和最後執行時間
            foreach (var task in tasks)
            {
                _channel.QueueDeclare(
                    queue: task.Handler.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _lastRunTimes[task.Handler.TaskName] = DateTime.MinValue;
            }

            var taskRunners = tasks.Select(task => RunTaskAsync(task, stoppingToken)).ToArray();
            await Task.WhenAll(taskRunners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteAsync");
            throw;
        }
    }

    private async Task RunTaskAsync(TaskHandlerConfig task, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = GetNextRunTime(task.Config, _lastRunTimes[task.Handler.TaskName], now);

                if (nextRun > now)
                {
                    var delay = nextRun - now;
                    LogScheduleInfo(task, nextRun, delay);
                    await Task.Delay(delay, stoppingToken);
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    await SendTask(task.Handler);
                    _lastRunTimes[task.Handler.TaskName] = nextRun;
                    _logger.LogInformation($"Task {task.Handler.TaskName} executed successfully at {DateTime.UtcNow}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing task {task.Handler.TaskName}");
                // 等待短暫時間後重試
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private DateTime GetNextRunTime(TaskConfig config, DateTime lastRun, DateTime now)
    {
        try
        {
            ValidateConfiguration(config);

            var interval = config.Interval?.ToTimeSpan();
            if (!interval.HasValue)
            {
                throw new InvalidOperationException($"Task {config.TaskName} must specify an Interval");
            }

            // 對於首次執行的處理
            if (lastRun == DateTime.MinValue)
            {
                // 如果設置了特定時間
                if (!string.IsNullOrEmpty(config.TimeOfDay))
                {
                    var (hour, minute) = ParseTimeOfDay(config.TimeOfDay);
                    var firstRun = now.Date.AddHours(hour).AddMinutes(minute);

                    // 如果今天的執行時間已過，計算下一個間隔
                    if (firstRun <= now)
                    {
                        firstRun = firstRun.Add(interval.Value);
                    }

                    return firstRun;
                }

                return now; // 對於純間隔任務，立即開始
            }

            // 計算基本的下次執行時間
            var nextRun = lastRun.Add(interval.Value);

            // 如果設置了特定時間，調整到指定時間點
            if (!string.IsNullOrEmpty(config.TimeOfDay))
            {
                var (hour, minute) = ParseTimeOfDay(config.TimeOfDay);
                
                // 檢查是否是每小時固定分鐘的格式 (XX:MM)
                if (hour == -1)  // -1 表示使用 XX 作為小時佔位符
                {
                    // 保留當前小時，只調整分鐘
                    nextRun = new DateTime(
                        nextRun.Year,
                        nextRun.Month,
                        nextRun.Day,
                        nextRun.Hour,
                        minute,
                        0);
                }
                else
                {
                    // 常規每日固定時間格式
                    nextRun = nextRun.Date.AddHours(hour).AddMinutes(minute);
                }
            }

            // 確保下次執行時間在當前時間之後
            while (nextRun <= now)
            {
                nextRun = nextRun.Add(interval.Value);
            }

            return nextRun;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calculating next run time for task {config.TaskName}");
            throw;
        }
    }

    private (int Hour, int Minute) ParseTimeOfDay(string timeOfDay)
    {
        var parts = timeOfDay.Split(':');
        if (parts.Length != 2)
        {
            throw new FormatException($"Invalid TimeOfDay format: {timeOfDay}. Expected format: HH:mm or XX:mm");
        }

        // 檢查是否是每小時格式 (XX:mm)
        if (parts[0].Equals("XX", StringComparison.OrdinalIgnoreCase))
        {
            // 返回特殊值 -1 表示「每小時」
            if (!int.TryParse(parts[1], out int minute) || minute < 0 || minute > 59)
            {
                throw new FormatException($"Invalid minute format: {parts[1]}. Expected range: 0-59");
            }
            return (-1, minute);
        }
        else
        {
            // 常規時間格式 (HH:mm)
            if (!int.TryParse(parts[0], out int hour) ||
                !int.TryParse(parts[1], out int minute) ||
                hour < 0 || hour > 23 || minute < 0 || minute > 59)
            {
                throw new FormatException($"Invalid TimeOfDay format: {timeOfDay}. Expected format: HH:mm");
            }
            return (hour, minute);
        }
    }

    private void ValidateConfigurations(List<TaskConfig> configs)
    {
        if (configs == null || !configs.Any())
        {
            throw new InvalidOperationException("No tasks configured");
        }

        foreach (var config in configs)
        {
            ValidateConfiguration(config);
        }
    }

    private void ValidateConfiguration(TaskConfig config)
    {
        if (string.IsNullOrEmpty(config.TaskName))
        {
            throw new InvalidOperationException("TaskName is required");
        }

        if (config.Interval == null)
        {
            throw new InvalidOperationException($"Task {config.TaskName} must specify an Interval");
        }

        if (!string.IsNullOrEmpty(config.TimeOfDay))
        {
            // 驗證時間格式
            ParseTimeOfDay(config.TimeOfDay);
        }
    }

    private void LogScheduleInfo(TaskHandlerConfig task, DateTime nextRun, TimeSpan delay)
    {
        var scheduleType = GetScheduleDescription(task.Config);
        _logger.LogInformation(
            $"Task {task.Handler.TaskName} scheduled at: {nextRun:yyyy-MM-dd HH:mm:ss} ({scheduleType}). " +
            $"Waiting for {delay.TotalMinutes:F1} minutes");
    }

    private string GetScheduleDescription(TaskConfig config)
    {
        var interval = config.Interval?.ToTimeSpan();
        if (!interval.HasValue)
        {
            return "Invalid configuration";
        }

        if (string.IsNullOrEmpty(config.TimeOfDay))
        {
            return $"Every {FormatTimeSpan(interval.Value)}";
        }

        // 檢查是否是每小時固定分鐘格式
        if (config.TimeOfDay.StartsWith("XX:", StringComparison.OrdinalIgnoreCase))
        {
            return $"Every {FormatTimeSpan(interval.Value)} at minute {config.TimeOfDay.Substring(3)}";
        }

        return $"Every {FormatTimeSpan(interval.Value)} at {config.TimeOfDay}";
    }

    private string FormatTimeSpan(TimeSpan span)
    {
        if (span.Days > 0)
        {
            return $"{span.Days} day(s)";
        }
        if (span.Hours > 0)
        {
            return $"{span.Hours} hour(s)";
        }
        if (span.Minutes > 0)
        {
            return $"{span.Minutes} minute(s)";
        }
        return $"{span.Seconds} second(s)";
    }

    private async Task SendTask(ITaskHandler handler)
    {
        try
        {
            var message = $"Execute {handler.TaskName}";
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "",
                routingKey: handler.QueueName,
                basicProperties: null,
                body: body);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending message for task {handler.TaskName}");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            await base.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping MQTaskScheduler");
            throw;
        }
    }
}
