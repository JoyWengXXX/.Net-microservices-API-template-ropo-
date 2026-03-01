using CQRS.Core.Events;

namespace UserRoleService.Cmd.Domain.Events
{
    public class UpdateRoleEvent : BaseEvent
    {
        public UpdateRoleEvent() : base(nameof(UpdateRoleEvent))
        {
        }

        public Guid roleId { get; set; }
        public int roleOrder { get; set; }
        public string roleName { get; set; }
    }
}

