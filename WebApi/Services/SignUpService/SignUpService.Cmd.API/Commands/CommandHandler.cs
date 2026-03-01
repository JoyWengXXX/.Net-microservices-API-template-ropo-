using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using SignUpService.Cmd.API.Commands.Interfaces;
using SignUpService.Cmd.Domain.Aggregates;

namespace SignUpService.Cmd.API.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IEventSourcingHandler<SignUpAggregate> _eventSourcingHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly bool _isEventSourcingEnabled = false;

        public CommandHandler(IEventSourcingHandler<SignUpAggregate> eventSourcingHandler,
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration)
        {
            _eventSourcingHandler = eventSourcingHandler;
            _httpContextAccessor = httpContextAccessor;
            _isEventSourcingEnabled = configuration.GetValue<bool>("EventSouringEnable");
        }

        public async Task<TResult> HandleAsync(SignInCommand command)
        {
            var aggregate = new SignUpAggregate(_httpContextAccessor, command.id, command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(ValidateCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.Validate(command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(ForgetPasswordCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.ForgetPassword(command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(PasswordChangeCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.PasswordChange(command.newPW);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(UserInfoChangeCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.UserInfoChange(command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(AccountDisableCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.AccountDisable(command.userId);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(AccountDeleteCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.AccountDelete(command.userId);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }
    }
}

