using CQRS.Core.Infrastructure;

namespace RolePermissionService.Cmd.API.Commands.Interfaces
{
    public interface ICommandHandler
    {
        Task<TResult> HandleAsync(UpdateRolePermissionCommand command);
    }
}

