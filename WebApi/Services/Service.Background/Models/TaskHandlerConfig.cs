using Service.Background.Services.Interfaces;

namespace Service.Background.Models
{
    public class TaskHandlerConfig
    {
        public ITaskHandler Handler { get; set; }
        public TaskConfig Config { get; set; }
    }
}

