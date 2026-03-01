using CQRS.Core.Events;

namespace LogInService.Cmd.Domain.Events
{
    public class GoogleLogInEvent : BaseEvent
    {
        public GoogleLogInEvent() : base(nameof(GoogleLogInEvent))
        {
        }

        public string email { get; set; }
        public string name { get; set; }
        public string photoUrl { get; set; }
    }
}

