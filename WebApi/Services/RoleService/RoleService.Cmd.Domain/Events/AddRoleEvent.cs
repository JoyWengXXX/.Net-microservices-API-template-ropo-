using CQRS.Core.Events;

namespace UserRoleService.Cmd.Domain.Events
{
    public class AddRoleEvent : BaseEvent
    {
        public AddRoleEvent() : base(nameof(AddRoleEvent))
        {
        }

        public int roleOrder { get; set; }
        public string roleName { get; set; }
    }
}

