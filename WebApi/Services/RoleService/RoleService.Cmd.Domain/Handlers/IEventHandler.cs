using UserRoleService.Cmd.Domain.Events;
using CQRS.Core.Infrastructure;

namespace RoleService.Cmd.Domain.Handlers
{
    public interface IEventHandler
    {
        Task<TResult> On(AddRoleEvent @event);
        Task<TResult> On(UpdateRoleEvent @event);
        Task<TResult> On(DisableRoleEvent @event);
    }
}

