using CQRS.Core.Commands;
using RoleService.Cmd.Domain.DTOs;

namespace RoleService.Cmd.API.Commands
{
    public class UpdateRoleCommand : BaseCommand
    {
        public UpdateRoleDTO input { get; set; }
    }
}

