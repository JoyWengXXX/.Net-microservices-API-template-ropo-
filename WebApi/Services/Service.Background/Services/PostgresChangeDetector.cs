
public class PostgresChangeDetector : IDisposable, IPostgresChangeDetector
{
    private readonly IRepository<MainDBConnectionManager> _repo;
    private NpgsqlConnection _listenConnection;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger<PostgresChangeDetector> _logger;
    private bool _isListening;
    public event EventHandler<string> OnDataChanged;

    public PostgresChangeDetector(
        IRepository<MainDBConnectionManager> repo,
        ILogger<PostgresChangeDetector> logger)
    {
        _repo = repo;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartListening(string tableName)
    {
        try
        {
            //確認觸發器的建立
            await CreateTrigger(tableName);

            _logger.LogInformation($"Starting listening process for table: {tableName}");

            // 使用與現有觸發器相同的大小寫
            var channelName = $"{tableName}_changes";
            _logger.LogInformation($"Channel name: {channelName}");

            // 建立新的監聽連線
            _listenConnection = (NpgsqlConnection)_repo.GetConnection();

            _logger.LogInformation($"Connection state: {_listenConnection.State}");

            // 註冊通知頻道 - 使用引號包裹channel名稱以保持大小寫
            _logger.LogInformation($"Registering LISTEN on channel: {channelName}");
            using var cmd = new NpgsqlCommand($"LISTEN \"{channelName}\";", _listenConnection);
            await cmd.ExecuteNonQueryAsync();

            // 設置通知處理器
            _listenConnection.Notification += (sender, args) =>
            {
                _logger.LogInformation($"Received notification on channel: {args.Channel}");
                _logger.LogInformation($"Notification payload: {args.Payload}");
                OnDataChanged?.Invoke(this, args.Payload);
            };

            _isListening = true;
            _logger.LogInformation("Successfully started listening for notifications");

            // 持續等待通知
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await _listenConnection.WaitAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Listening cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while waiting for notifications");
                    // 嘗試重新連接
                    await ReconnectAsync(channelName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in StartListening for table {tableName}");
            throw;
        }
    }

    private async Task ReconnectAsync(string channelName)
    {
        try
        {
            _logger.LogInformation("Attempting to reconnect...");
            if (_listenConnection?.State == System.Data.ConnectionState.Open)
            {
                await _listenConnection.CloseAsync();
            }
            await _listenConnection.OpenAsync();
            using var cmd = new NpgsqlCommand($"LISTEN \"{channelName}\";", _listenConnection);
            await cmd.ExecuteNonQueryAsync();
            _logger.LogInformation("Successfully reconnected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reconnect");
            throw;
        }
    }

    private async Task CreateTrigger(string tableName)
    {
        try
        {
            using var connection = (NpgsqlConnection)_repo.GetConnection();

            // 建立觸發器函數
            var createFunctionSql = @"
                CREATE OR REPLACE FUNCTION notify_table_changes()
                RETURNS trigger AS $$
                DECLARE
                    payload text;
                BEGIN
                    RAISE NOTICE 'Trigger fired for operation: % on table: %', TG_OP, TG_TABLE_NAME;
                    
                    IF (TG_OP = 'DELETE') THEN
                        payload = json_build_object(
                            'operation', TG_OP,
                            'schema', TG_TABLE_SCHEMA,
                            'table', TG_TABLE_NAME,
                            'record', row_to_json(OLD)
                        )::text;
                    ELSE
                        payload = json_build_object(
                            'operation', TG_OP,
                            'schema', TG_TABLE_SCHEMA,
                            'table', TG_TABLE_NAME,
                            'record', row_to_json(NEW)
                        )::text;
                    END IF;
                    
                    RAISE NOTICE 'Sending notification with payload: %', payload;
                    -- 使用與表名相同的大小寫來發送通知
                    PERFORM pg_notify(TG_TABLE_NAME || '_changes', payload);
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;";

            using var cmdFunction = new NpgsqlCommand(createFunctionSql, connection);
            await cmdFunction.ExecuteNonQueryAsync();
            _logger.LogInformation("Trigger function created successfully");

            // 使用與現有觸發器相同的命名方式
            var triggerName = $"{tableName}_notify_trigger";
            var createTriggerSql = $@"
                DROP TRIGGER IF EXISTS ""{triggerName}"" ON ""{tableName}"";
                CREATE TRIGGER ""{triggerName}""
                AFTER INSERT OR UPDATE OR DELETE ON ""{tableName}""
                FOR EACH ROW EXECUTE FUNCTION notify_table_changes();";

            using var cmdTrigger = new NpgsqlCommand(createTriggerSql, connection);
            await cmdTrigger.ExecuteNonQueryAsync();
            _logger.LogInformation("Trigger created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating trigger for table {tableName}");
            throw;
        }
    }

    public void StopListening()
    {
        _logger.LogInformation("Stopping listening process");
        _cancellationTokenSource.Cancel();
        _isListening = false;

        if (_listenConnection?.State == System.Data.ConnectionState.Open)
        {
            _listenConnection.Close();
            _logger.LogInformation("Connection closed");
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing PostgresChangeDetector");
        if (_isListening)
        {
            StopListening();
        }
        _cancellationTokenSource.Cancel();
        _listenConnection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
