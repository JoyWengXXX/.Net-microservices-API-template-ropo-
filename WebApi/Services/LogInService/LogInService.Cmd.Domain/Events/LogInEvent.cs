using CQRS.Core.Events;

namespace LogInService.Cmd.Domain.Events
{
    public class LogInEvent : BaseEvent
    {
        public LogInEvent() : base(nameof(LogInEvent))
        {
        }

        public string userAccount { get; set; }
        public string password { get; set; }
        public DateTime logInDate { get; set; }
    }
}

