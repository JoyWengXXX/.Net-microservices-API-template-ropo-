using System.Text.Json;
using System.Text;

namespace Service.Background.Services
{
    internal class DbConnectionMonitorService : IDbConnectionMonitorService
    {
        private int maxPoolSize = 100; //?è¨­?€å¤§é€????        private string _discordWebhookUrl;
        private string _environmentName;
        private float _connectionWarningThreshold;
        private DateTime _lastStatusSent = DateTime.MinValue;

        //è­¦å??ç¤º?·å»?‚é?ï¼Œé˜²æ­¢é??¼é »ç¹ç™¼??        private DateTime _lastWarningSent = DateTime.MinValue;
        private readonly TimeSpan _warningCooldown = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;

        private bool _isInitialized = false;
        public DbConnectionMonitorService(IServiceScopeFactory scopeFactory,
                                            HttpClient httpClient,
                                            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task InitalizeDbConnectionMonitorAsync()
        {
            Console.WriteLine("?å??–DbConnection??§ç¨‹å?");
            using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<MainDBConnectionManager>>();

            // ?´æ¥å¾è¨­å®šæ??–å??®å??°å??„è¨­å®šï??±ç’°å¢ƒå??‰ç? appsettings.{env}.json æ±ºå?ï¼?            var envSettings = _configuration.GetSection("EnvironmentSettings").Get<List<EnvironmentConfig>>();
            if (envSettings == null || !envSettings.Any())
            {
                Console.WriteLine("?¾ä??°ä»»ä½?EnvironmentSettings è¨­å?ï¼Œè?æª¢æŸ¥è¨­å?æª”ã€?);
                return;
            }

            var matched = envSettings.First();
            _discordWebhookUrl = matched.DiscordWebHookUrl;
            _environmentName = matched.EnvironmentName;
            _connectionWarningThreshold = matched.ConnectionWarningThreshold;
            Console.WriteLine($"?®å??„ç’°å¢ƒç‚º : {_environmentName}");

            var sql = @"SELECT current_setting('max_connections') AS MaxConnections;";
            try
            {
                var result = await repo.ComplexQueryAsync<DbMaxConnection>(sql);
                if (result.Any())
                {
                    maxPoolSize = int.Parse(result.First().MaxConnections);
                    Console.WriteLine($"è³‡æ?åº«æ?å¤§é€???¸ï?{maxPoolSize}");
                    //?¥ç„¡æ³•å?å¾—æ?å¤§é€???¸ï?è¦é??šInitalize
                    _isInitialized = true;
                }
                else
                {
                    Console.WriteLine($"MaxConnections?¡æ??–å? resultï¼š{result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error]?¡æ??–å?è³‡æ?åº«æ?å¤§é€???¸ï??¯èª¤è¨Šæ¯ï¼š{ex.Message}");
                return;
            }
        }
        public async Task DbConnectionMonitorAsync()
        {
            if(!_isInitialized)
            {
                //å°šæœª?å??–ï??ˆåŸ·è¡Œå?å§‹å?
                await InitalizeDbConnectionMonitorAsync();
                return;
            }
            int active = 0;
            int idle = 0;
            int total = 0;
            int idle_in_txn = 0;
            var sql = @"SELECT
                        count(*) FILTER (WHERE state = 'active') AS active,
                        count(*) FILTER (WHERE state = 'idle') AS idle,
                        count(*) AS total,
						count(*) FILTER (WHERE state = 'idle in transaction') AS idle_in_txn
                      FROM pg_stat_activity
                      ";
            using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<MainDBConnectionManager>>();

            var status = await repo.ComplexQueryAsync<DbConnectionStatus>(sql);
            if (status.Any())
            {
                var stat = status.First();
                idle_in_txn = stat.Idle_in_txn;
                active = stat.Active;
                idle = stat.Idle;
                total = stat.Total;
            }
            else
            {
                Console.WriteLine($"?¡æ??–å?????€??statusï¼š{status}");
                return;
            }
            // æ¯å??‚ç™¼?ä?æ¬¡ç???            if (DateTime.UtcNow - _lastStatusSent > TimeSpan.FromHours(1))
            {
                _lastStatusSent = DateTime.UtcNow;
                await SendDiscordMessageAsync($"[{_environmentName}-å®šæ??å ±] ?®å?????¸ï?{total}/{maxPoolSize} (Active: {active}, Idle: {idle}, IdleInTxn: {idle_in_txn})");
            }
            //????¸è???0%è­¦ç¤º
            if (total >= maxPoolSize * _connectionWarningThreshold)
            {
                if (DateTime.UtcNow - _lastWarningSent > _warningCooldown)
                {
                    await SendDiscordMessageAsync($"@everyone [{_environmentName}-?°å¸¸?å ±] DB????¸è??æ?å¤§å€¼{_connectionWarningThreshold * 100:F0}%ï¼ï?ç¸½æ•¸/?€å¤§ï? {total}/{maxPoolSize} (Active: {active}, Idle: {idle}, IdleInTxn: {idle_in_txn})");
                    Console.WriteLine($"[Warning]DB????¸è??æ?å¤§å€¼{_connectionWarningThreshold * 100:F0}%ï¼Œç¸½???€å¤§ï? {total}/{maxPoolSize} (Active: {active}, Idle: {idle}, IdleInTxn: {idle_in_txn})");
                    _lastWarningSent = DateTime.UtcNow;
                }
            }
            else
            {
                Console.WriteLine($"DB????¸æ­£å¸¸ï?ç¸½æ•¸/?€å¤§ï? {total}/{maxPoolSize} (Active: {active}, Idle: {idle}, IdleInTxn: {idle_in_txn})");
            }
        }
        private async Task SendDiscordMessageAsync(string message)
        {
            try
            {
                var payload = new { content = message };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_discordWebhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Discord è¨Šæ¯?¼é€æ???);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Discord è¨Šæ¯?¼é€å¤±?—ï??€?‹ç¢¼: {response.StatusCode}ï¼Œå…§å®? {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?¼é€?Discord è¨Šæ¯?‚ç™¼?ŸéŒ¯èª? {ex.Message}");
            }
        }
    }
}

