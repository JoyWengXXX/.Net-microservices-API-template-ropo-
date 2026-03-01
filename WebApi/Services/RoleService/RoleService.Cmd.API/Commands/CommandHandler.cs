using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using RoleService.Cmd.API.Commands.Interfaces;
using RoleService.Cmd.Domain.Aggregates;

namespace RoleService.Cmd.API.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IEventSourcingHandler<RoleAggregate> _eventSourcingHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly bool _isEventSourcingEnabled = false;

        public CommandHandler(IEventSourcingHandler<RoleAggregate> eventSourcingHandler, 
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration)
        {
            _eventSourcingHandler = eventSourcingHandler;
            _httpContextAccessor = httpContextAccessor;
            _isEventSourcingEnabled = configuration.GetValue<bool>("EventSouringEnable");
        }

        public async Task<TResult> HandleAsync(AddRoleCommand command)
        {
            var aggregate = new RoleAggregate(_httpContextAccessor, command.id, command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(UpdateRoleCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.UpdateRole(command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }

        public async Task<TResult> HandleAsync(DisableRoleCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.id, _isEventSourcingEnabled);
            aggregate.DisableRole(command.roleId);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }
    }
}

