using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service.Background.Services.Interfaces;
using System.Text;
using Service.Background.Services;
using SystemMain.Entities;
using System.Text.Json;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private IModel _channel;
    private IPostgresChangeDetector _changeDetector;
    private IFcmNotificationService _fcmNotification;

    public Worker(ILogger<Worker> logger
                    ,IConnection connection
                    ,IServiceProvider serviceProvider)
    {
        _logger = logger;
        _connection = connection;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // ??ÅΩ RabbitMQ Ë®äÊÅØ
            await StartRabbitMQListening(stoppingToken);

            // ??ÅΩ PostgreSQL ËÆäÊõ¥
            await StartPostgresListening(stoppingToken);

            // ‰øùÊ??çÂ??ãË?
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Worker ExecuteAsync");
            throw;
        }
    }

    private async Task StartRabbitMQListening(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();

        using (var scope = _serviceProvider.CreateScope())
        {
            var taskHandlers = scope.ServiceProvider.GetRequiredService<IEnumerable<ITaskHandler>>();
            foreach (var handler in taskHandlers)
            {
                _channel.QueueDeclare(
                    queue: handler.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) => await ProcessMessage(ea, handler.GetType());
                _channel.BasicConsume(queue: handler.QueueName, autoAck: false, consumer: consumer);

                _logger.LogInformation("Started listening to RabbitMQ queue: {QueueName}", handler.QueueName);
            }
        }
    }

    private async Task StartPostgresListening(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            _changeDetector = scope.ServiceProvider.GetRequiredService<IPostgresChangeDetector>();
            _fcmNotification = scope.ServiceProvider.GetRequiredService<IFcmNotificationService>();

            // Ë®ªÂ? PostgreSQL ËÆäÊõ¥Ë≥áÊ??ïÁ?
            _changeDetector.OnDataChanged += async (sender, payload) =>
            {
                // ?®ÈÄôË£°?ïÁ?Ë≥áÊ?ËÆäÊõ¥‰∫ã‰ª∂
                try
                {
                    // Ëß??JSONÂ≠ó‰∏≤
                    using JsonDocument document = JsonDocument.Parse(payload);
                    JsonElement root = document.RootElement;

                    // Ê™¢Êü•operationÈ°ûÂ?
                    if (root.GetProperty("operation").GetString() == "INSERT")
                    {
                        _logger.LogInformation("Pcb Operation recorded!");
                        JsonElement recordElement = root.GetProperty("record");
                        var record = JsonSerializer.Deserialize<PcbOperateRecord>(recordElement.GetRawText());
                        //?ºÈÄÅÊ??®ÈÄöÁü•?®Êí≠
                        var result = await _fcmNotification.SendNotificationAsync(record);
                        result.ForEach(x => _logger.LogError(x));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing database change");
                }
            };

            // ÂæûË®≠ÂÆöÊ??∂‰?‰æÜÊ?Ê±∫Â??ÄË¶ÅÁõ£?ΩÁ?Ë≥áÊ?Ë°?
            var tablesToMonitor = new[] { typeof(PcbOperateRecord).Name }; // ?ôË£°Ë®≠ÁΩÆ?ëÈ?Ë¶ÅÁõ£?ΩÁ?Ë≥áÊ?Ë°?

            foreach (var table in tablesToMonitor)
            {
                try
                {
                    // ‰ΩøÁî® Task.Run ‰æÜÈÅø?çÈòªÂ°û‰∏ªÁ∑öÁ?
                    await Task.Run(async () =>
                    {
                        await _changeDetector.StartListening(table);
                        _logger.LogInformation($"Started monitoring PostgreSQL table: {table}");
                    }, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to start monitoring table: {table}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting PostgreSQL monitoring");
            throw;
        }
    }

    private async Task ProcessMessage(BasicDeliverEventArgs ea, Type handlerType)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        using (var scope = _serviceProvider.CreateScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService(handlerType) as ITaskHandler;
            if (handler == null)
            {
                _logger.LogError("Failed to resolve handler of type {HandlerType}", handlerType.Name);
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            _logger.LogInformation("Received message for task {TaskName}: {message}", handlerType.Name, message);

            try
            {
                await handler.ExecuteAsync();
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                _logger.LogInformation("Task {TaskName} completed successfully", handlerType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for task {TaskName}", handlerType.Name);
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping worker service...");

        // ?úÊ≠¢ PostgreSQL ??ÅΩ
        if (_changeDetector != null)
        {
            _changeDetector.StopListening();
            (_changeDetector as IDisposable)?.Dispose();
        }

        // ?úÈ? RabbitMQ ??é•
        _channel?.Close();
        _connection?.Close();

        await base.StopAsync(cancellationToken);
    }
}
