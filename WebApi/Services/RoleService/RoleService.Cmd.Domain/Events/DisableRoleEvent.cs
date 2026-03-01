using CQRS.Core.Events;

namespace UserRoleService.Cmd.Domain.Events
{
    public class DisableRoleEvent : BaseEvent
    {
        public DisableRoleEvent() : base(nameof(DisableRoleEvent))
        {
        }

        public Guid roleId { get; set; }
    }
}

