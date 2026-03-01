using CQRS.Core.Events;

namespace LogInService.Cmd.Domain.Events
{
    public class EnableNotificationEvent : BaseEvent
    {
        public EnableNotificationEvent() : base(nameof(EnableNotificationEvent))
        {
        }

        public int messageType { get; set; }
        public bool enable { get; set; }
        public string? subscription { get; set; }
    }
}

