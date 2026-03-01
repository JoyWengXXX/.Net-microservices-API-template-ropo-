using Service.Background.Services.Interfaces;

namespace Service.Background.Handlers
{
    internal class DbConnectionMonitorAsyncTaskHandler : ITaskHandler
    {
        private readonly IDbConnectionMonitorService _service;

        public DbConnectionMonitorAsyncTaskHandler(IDbConnectionMonitorService service)
        {
            _service = service;
        }
        public string TaskName { get; set; }
        public string QueueName => $"{TaskName}_queue";

        public async Task ExecuteAsync()
        {
            await _service.DbConnectionMonitorAsync();
        }
    }
}

