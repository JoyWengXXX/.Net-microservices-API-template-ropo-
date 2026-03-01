using CQRS.Core.Events;

namespace SignUpService.Cmd.Domain.Events
{
    public class PasswordChangeEvent : BaseEvent
    {
        public PasswordChangeEvent() : base(nameof(PasswordChangeEvent))
        {
        }

        public string newPassword { get; set; }
    }
}

