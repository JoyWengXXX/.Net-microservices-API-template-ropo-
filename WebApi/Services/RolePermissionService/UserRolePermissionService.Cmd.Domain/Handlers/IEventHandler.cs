using UserRolePermissionService.Cmd.Domain.Events;
using CQRS.Core.Infrastructure;

namespace RolePermissionService.Cmd.Domain.Handlers
{
    public interface IEventHandler
    {
        Task<TResult> On(UpdateRolePermissionEvent @event);
    }
}

