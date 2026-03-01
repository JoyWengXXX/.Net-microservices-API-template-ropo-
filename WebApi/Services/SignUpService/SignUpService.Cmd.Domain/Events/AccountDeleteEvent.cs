using CQRS.Core.Events;

namespace SignUpService.Cmd.Domain.Events
{
    public class AccountDeleteEvent : BaseEvent
    {
        public AccountDeleteEvent() : base(nameof(AccountDeleteEvent))
        {
        }

        public string userId { get; set; }
    }
}

