using CQRS.Core.Commands;

namespace RoleService.Cmd.API.Commands
{
    public class DisableRoleCommand : BaseCommand
    {
        public Guid roleId { get; set; }
    }
}

