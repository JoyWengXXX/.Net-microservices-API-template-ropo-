using CQRS.Core.Infrastructure;

namespace ControllerService.Cmd.API.Commands.Interfaces
{
    public interface ICommandHandler
    {
        Task<TResult> HandleAsync(AddControllerCommand command);
        Task<TResult> HandleAsync(UpdateControllerCommand command);
        Task<TResult> HandleAsync(DisableControllerCommand command);
    }
}

