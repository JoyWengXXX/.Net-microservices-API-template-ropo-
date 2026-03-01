using CQRS.Core.Events;

namespace LogInService.Cmd.Domain.Events
{
    public class LogOutEvent : BaseEvent
    {
        public LogOutEvent() : base(nameof(LogOutEvent))
        {
        }

        public string token { get; set; }
        public DateTime logOutDate { get; set; }
    }
}

