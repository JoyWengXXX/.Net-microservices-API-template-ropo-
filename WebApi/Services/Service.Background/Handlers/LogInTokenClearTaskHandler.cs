using Service.Background.Services.Interfaces;

namespace Service.Background.Handlers
{
    public class LogInTokenClearTaskHandler : ITaskHandler
    {
        private readonly ILogInTokenClearService _service;

        public LogInTokenClearTaskHandler(ILogInTokenClearService service)
        {
            _service = service;
        }

        public string TaskName { get; set; }
        public string QueueName => $"{TaskName}_queue";

        public async Task ExecuteAsync()
        {
            await _service.ClearAllDisabledLoginToken();
        }
    }
}

