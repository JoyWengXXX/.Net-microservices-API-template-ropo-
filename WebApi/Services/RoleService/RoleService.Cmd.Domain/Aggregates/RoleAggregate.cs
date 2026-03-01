using CQRS.Core.Domain;
using UserRoleService.Cmd.Domain.Events;
using Microsoft.AspNetCore.Http;
using RoleService.Cmd.Domain.DTOs;

namespace RoleService.Cmd.Domain.Aggregates
{
    public class RoleAggregate : AggregateRoot
    {
        private bool _active;
        public bool Active { get => _active; set => _active = value; }

        public RoleAggregate(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }
        public RoleAggregate(IHttpContextAccessor httpContextAccessor, Guid messageId, AddRoleDTO input) : base(httpContextAccessor)
        {
            RaiseEvent(new AddRoleEvent
            {
                id = messageId,
                roleOrder = input.roleOrder,
                roleName = input.roleName,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(RoleAggregate)
            });
        }
        public void Apply(AddRoleEvent @event)
        {
            _id = @event.id;
            _active = true;
        }

        public void UpdateRole(UpdateRoleDTO input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new UpdateRoleEvent
            {
                id = _id,
                roleOrder = input.roleOrder,
                roleId = input.roleId,
                roleName = input.roleName,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(RoleAggregate)
            });
        }
        public void Apply(UpdateRoleEvent @event)
        {
            _id = @event.id;
        }

        public void DisableRole(Guid input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new DisableRoleEvent
            {
                id = _id,
                roleId = input,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(RoleAggregate)
            });
        }
        public void Apply(DisableRoleEvent @event)
        {
            _id = @event.id;
        }
    }
}

