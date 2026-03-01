using CQRS.Core.Domain;
using ControllerService.Cmd.Domain.Events;
using ControllerService.Cmd.Domain.DTOs;
using Microsoft.AspNetCore.Http;

namespace ControllerService.Cmd.Domain.Aggregates
{
    public class ControllerAggregate : AggregateRoot
    {
        private bool _active;
        public bool Active { get => _active; set => _active = value; }

        public ControllerAggregate(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }
        public ControllerAggregate(IHttpContextAccessor httpContextAccessor, Guid messageId, AddControllerDTO input) : base(httpContextAccessor)
        {
            RaiseEvent(new AddControllerEvent
            {
                id = messageId,
                controllerId = input.controllerId,
                controllerName = input.controllerName,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(ControllerAggregate)
            });
        }
        public void Apply(AddControllerEvent @event)
        {
            _id = @event.id;
            _active = true;
        }

        public void UpdateController(UpdateControllerDTO input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new UpdateControllerEvent
            {
                id = _id,
                controllerId = input.controllerId,
                controllerName = input.controllerName,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(ControllerAggregate)
            });
        }
        public void Apply(UpdateControllerEvent @event)
        {
            _id = @event.id;
        }

        public void DisableController(string input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new DisableControllerEvent
            {
                id = _id,
                controllerId = input,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(ControllerAggregate)
            });
        }
        public void Apply(DisableControllerEvent @event)
        {
            _id = @event.id;
        }
    }
}

