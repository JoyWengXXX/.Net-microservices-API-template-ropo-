using CQRS.Core.Events;

namespace UserRolePermissionService.Cmd.Domain.Events
{
    public class UpdateRolePermissionEvent : BaseEvent
    {
        public UpdateRolePermissionEvent() : base(nameof(UpdateRolePermissionEvent))
        {
        }

        public string userId { get; set; }
        public Guid storeId { get; set; }
        public Guid roleId { get; set; }

        public List<UserRoleBinding> permissions { get; set; }
    }

    public partial class UserRoleBinding
    {
        public string ControllerId { get; set; }
        public bool QueryAllowed { get; set; }
        public bool CreateAllowed { get; set; }
        public bool UpdateAllowed { get; set; }
        public bool DeleteAllowed { get; set; }
    }
}
