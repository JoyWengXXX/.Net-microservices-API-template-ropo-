using CQRS.Core.Infrastructure;

namespace LogInService.Cmd.Commands.Interfaces
{
    public interface ICommandHandler
    {
        Task<TResult> HandleAsync(LogInCommand command);
        Task<TResult> HandleAsync(AdminLogInCommand command);
        Task<TResult> HandleAsync(LogOutCommand command);
        Task<TResult> HandleAsync(GoogleLogInCommand command);
        Task<TResult> HandleAsync(EnableNotificationCommand command);
    }
}

