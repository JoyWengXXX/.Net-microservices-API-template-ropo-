using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Background.Services.Interfaces
{
    public interface ITaskHandler
    {
        string TaskName { get; set; }
        string QueueName { get; }
        Task ExecuteAsync();
    }
}

