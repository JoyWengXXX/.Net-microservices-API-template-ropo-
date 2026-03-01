using ControllerService.Cmd.Domain.Events;
using CQRS.Core.Infrastructure;

namespace ControllerService.Cmd.Domain.Handlers
{
    public interface IEventHandler
    {
        Task<TResult> On(AddControllerEvent @event);
        Task<TResult> On(UpdateControllerEvent @event);
        Task<TResult> On(DisableControllerEvent @event);
    }
}

