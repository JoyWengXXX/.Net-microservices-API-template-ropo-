using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using ControllerService.Cmd.API.Commands.Interfaces;
using ControllerService.Cmd.Domain.Aggregates;

namespace ControllerService.Cmd.API.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IEventSourcingHandler<ControllerAggregate> _eventSourcingHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly bool _isEventSourcingEnabled;

        public CommandHandler(IEventSourcingHandler<ControllerAggregate> eventSourcingHandler, 
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration)
        {
            _eventSourcingHandler = eventSourcingHandler;
            _httpContextAccessor = httpContextAccessor;
            _isEventSourcingEnabled= configuration.GetValue<bool>("EventSouringEnable");
        }

        public async Task<TResult> HandleAsync(AddControllerCommand command)
        {
            var aggregate = new ControllerAggregate(_httpContextAccessor, command.id, command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(UpdateControllerCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.UpdateController(command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(DisableControllerCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.DisableController(command.ControllerId);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }
    }
}

