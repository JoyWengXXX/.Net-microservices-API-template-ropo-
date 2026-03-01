using CQRS.Core.Events;

namespace LogInService.Cmd.Domain.Events
{
    public class AdminLogInEvent : BaseEvent
    {
        public AdminLogInEvent() : base(nameof(AdminLogInEvent))
        {
        }

        public string userAccount { get; set; }
        public string password { get; set; }
        public DateTime logInDate { get; set; }
    }
}

