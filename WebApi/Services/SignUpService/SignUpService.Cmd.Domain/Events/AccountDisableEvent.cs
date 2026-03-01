using CQRS.Core.Events;

namespace SignUpService.Cmd.Domain.Events
{
    public class AccountDisableEvent : BaseEvent
    {
        public AccountDisableEvent() : base(nameof(AccountDisableEvent))
        {
        }

        public string userId { get; set; }
    }
}

