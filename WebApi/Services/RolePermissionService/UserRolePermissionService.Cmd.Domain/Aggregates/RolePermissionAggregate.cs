using AutoMapper;
using CQRS.Core.Domain;
using UserRolePermissionService.Cmd.Domain.Events;
using Microsoft.AspNetCore.Http;
using RolePermissionService.Cmd.Domain.DTOs;
using RolePermissionService.Cmd.Domain.Mappers;

namespace RolePermissionService.Cmd.Domain.Aggregates
{
    public class RolePermissionAggregate : AggregateRoot
    {
        private bool _active;
        public bool Active { get => _active; set => _active = value; }

        public RolePermissionAggregate(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }
        public RolePermissionAggregate(IHttpContextAccessor httpContextAccessor, UpdateRolePermissionDTO input) : base(httpContextAccessor)
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<RolePermissionProfile>());
            var mapper = config.CreateMapper();
            RaiseEvent(new UpdateRolePermissionEvent
            {
                id = _id,
                userId = input.userId,
                storeId = input.storeId,
                roleId = input.roleId,
                permissions = mapper.Map<List<UserRoleBinding>>(input.permissions),
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(RolePermissionAggregate)
            });
        }
        public void Apply(UpdateRolePermissionEvent @event)
        {
            _id = @event.id;
            _active = true;
        }
    }
}

