using CQRS.Core.Commands;
using CQRS.Core.Infrastructure;

namespace RoleService.Cmd.API.Commands.Interfaces
{
    public interface ICommandHandler
    {
        Task<TResult> HandleAsync(AddRoleCommand command);
        Task<TResult> HandleAsync(UpdateRoleCommand command);
        Task<TResult> HandleAsync(DisableRoleCommand command);
    }
}

