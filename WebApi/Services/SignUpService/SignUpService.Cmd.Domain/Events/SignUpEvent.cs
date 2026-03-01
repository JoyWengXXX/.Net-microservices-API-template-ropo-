using CQRS.Core.Events;

namespace SignUpService.Cmd.Domain.Events
{
    public class SignUpEvent : BaseEvent
    {
        public SignUpEvent() : base(nameof(SignUpEvent))
        {
        }

        public string userId { get; set; }
        public string password { get; set; }
        public string name { get; set; }
    }
}

