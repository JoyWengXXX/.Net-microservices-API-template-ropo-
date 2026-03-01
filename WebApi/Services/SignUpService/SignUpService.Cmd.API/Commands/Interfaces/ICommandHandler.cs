using CQRS.Core.Infrastructure;

namespace SignUpService.Cmd.API.Commands.Interfaces
{
    public interface ICommandHandler
    {
        Task<TResult> HandleAsync(SignInCommand command);
        Task<TResult> HandleAsync(ValidateCommand command);
        Task<TResult> HandleAsync(ForgetPasswordCommand command);
        Task<TResult> HandleAsync(PasswordChangeCommand command);
        Task<TResult> HandleAsync(UserInfoChangeCommand command);
        Task<TResult> HandleAsync(AccountDisableCommand command);
        Task<TResult> HandleAsync(AccountDeleteCommand command);
    }
}

