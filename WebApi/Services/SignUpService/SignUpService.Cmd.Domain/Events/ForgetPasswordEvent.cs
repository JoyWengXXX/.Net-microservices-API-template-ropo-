using CQRS.Core.Events;

namespace SignUpService.Cmd.Domain.Events
{
    public class ForgetPasswordEvent : BaseEvent
    {
        public ForgetPasswordEvent() : base(nameof(ForgetPasswordEvent))
        {
        }

        public string userId { get; set; }
    }
}

