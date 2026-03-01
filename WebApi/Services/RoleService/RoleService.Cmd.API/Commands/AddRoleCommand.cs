using CQRS.Core.Commands;
using RoleService.Cmd.Domain.DTOs;

namespace RoleService.Cmd.API.Commands
{
    public class AddRoleCommand : BaseCommand
    {
        public AddRoleDTO input { get; set; }
    }
}

