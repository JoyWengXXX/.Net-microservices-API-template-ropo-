using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using LogInService.Cmd.Commands;
using LogInService.Cmd.Commands.Interfaces;
using LogInService.Cmd.Domain.Aggregates;

namespace LogInService.API.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IEventSourcingHandler<LogInAggregate> _eventSourcingHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly bool _isEventSourcingEnabled = false;

        public CommandHandler(IEventSourcingHandler<LogInAggregate> eventSourcingHandler, 
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration)
        {
            _eventSourcingHandler = eventSourcingHandler;
            _httpContextAccessor = httpContextAccessor;
           _isEventSourcingEnabled= configuration.GetValue<bool>("EventSouringEnable");
        }

        public async Task<TResult> HandleAsync(LogInCommand command)
        {
            var aggregate = new LogInAggregate(_httpContextAccessor, command.id, command.input, null, null);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(AdminLogInCommand command)
        {
            var aggregate = new LogInAggregate(_httpContextAccessor, command.id, null, command.input, null);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(GoogleLogInCommand command)
        {
            var aggregate = new LogInAggregate(_httpContextAccessor, command.id, null, null, command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(LogOutCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.LogOut(command.token);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(EnableNotificationCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.EnableSystemNotification(command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }
    }
}

