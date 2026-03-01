using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using RolePermissionService.Cmd.API.Commands.Interfaces;
using RolePermissionService.Cmd.Domain.Aggregates;

namespace RolePermissionService.Cmd.API.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IEventSourcingHandler<RolePermissionAggregate> _eventSourcingHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly bool _isEventSourcingEnabled = false;

        public CommandHandler(IEventSourcingHandler<RolePermissionAggregate> eventSourcingHandler, 
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration)
        {
            _eventSourcingHandler = eventSourcingHandler;
            _httpContextAccessor = httpContextAccessor;
            _isEventSourcingEnabled = configuration.GetValue<bool>("EventSouringEnable");
        }

        public async Task<TResult> HandleAsync(UpdateRolePermissionCommand command)
        {
            var aggregate = new RolePermissionAggregate(_httpContextAccessor, command.input);

            return await _eventSourcingHandler.SaveAsync(aggregate, _isEventSourcingEnabled);
        }
    }
}

