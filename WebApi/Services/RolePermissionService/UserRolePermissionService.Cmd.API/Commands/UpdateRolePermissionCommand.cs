using CQRS.Core.Commands;
using RolePermissionService.Cmd.Domain.DTOs;

namespace RolePermissionService.Cmd.API.Commands
{
    public class UpdateRolePermissionCommand : BaseCommand
    {
        public UpdateRolePermissionDTO input { get; set; }
    }
}

