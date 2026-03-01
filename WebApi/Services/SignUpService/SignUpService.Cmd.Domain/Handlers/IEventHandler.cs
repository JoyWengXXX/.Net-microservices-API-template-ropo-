using SignUpService.Cmd.Domain.Events;
using CQRS.Core.Infrastructure;

namespace SignUpService.Cmd.Domain.Handlers
{
    public interface IEventHandler
    {
        Task<TResult> On(SignUpEvent @event);
        Task<TResult> On(ValidateEvent @event);
        Task<TResult> On(ForgetPasswordEvent @event);
        Task<TResult> On(PasswordChangeEvent @event);
        Task<TResult> On(UserInfoChangeEvent @event);
        Task<TResult> On(AccountDisableEvent @event);
        Task<TResult> On(AccountDeleteEvent @event);
    }
}

