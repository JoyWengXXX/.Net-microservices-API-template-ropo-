using LogInService.Cmd.Domain.Events;
using CQRS.Core.Infrastructure;

namespace LogInService.Cmd.Domain.Handlers
{
    public interface IEventHandler
    {
        Task<TResult> On(LogInEvent @event);
        Task<TResult> On(AdminLogInEvent @event);
        Task<TResult> On(GoogleLogInEvent @event);
        Task<TResult> On(LogOutEvent @event);
    }
}

