using CQRS.Core.Events;

namespace SignUpService.Cmd.Domain.Events
{
    public class ValidateEvent : BaseEvent
    {
        public ValidateEvent() : base(nameof(ValidateEvent))
        {
        }

        public string userId { get; set; }
        public string validationCode { get; set; }
    }
}

