using CQRS.Core.Domain;
using LogInService.Cmd.Domain.Events;
using LogInService.Cmd.Domain.DTOs;
using LogInService.Cmd.Domain.Models;
using Microsoft.AspNetCore.Http;
using Service.Common.Middleware;

namespace LogInService.Cmd.Domain.Aggregates
{
    public class LogInAggregate : AggregateRoot
    {
        private bool _active;

        public bool Active { get => _active; set => _active = value; }

        public LogInAggregate(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }
        public LogInAggregate(IHttpContextAccessor httpContextAccessor, Guid messageId, LogInDTO? regularLogin, LogInDTO? adminLogin, GoogleCredentialPayload? payload): base(httpContextAccessor)
        {
            if(regularLogin != null)
            {
                RaiseEvent(new LogInEvent
                {
                    id = messageId,
                    userAccount = regularLogin.userId,
                    password = regularLogin.password,
                    logInDate = DateTime.UtcNow,
                    Type = nameof(LogInAggregate),
                });
            }
            else if(adminLogin != null)
            {
                RaiseEvent(new AdminLogInEvent
                {
                    id = messageId,
                    userAccount = adminLogin.userId,
                    password = adminLogin.password,
                    logInDate = DateTime.UtcNow,
                    Type = nameof(LogInAggregate),
                });
            }
            else if(payload != null)
            {
                RaiseEvent(new GoogleLogInEvent
                {
                    id = messageId,
                    email = payload.email,
                    name = payload.name,
                    photoUrl = payload.photo,
                    Type = nameof(LogInAggregate),
                });
            }
            else
            { throw new AppException("Login info is not complete"); }
        }
        public void Apply(LogInEvent @event)
        {
            _id = @event.id;
            _active = true;
        }
        public void Apply(AdminLogInEvent @event)
        {
            _id = @event.id;
            _active = true;
        }
        public void Apply(GoogleLogInEvent @event)
        {
            _id = @event.id;
            _active = true;
        }

        public void LogOut(string token)
        {
            if (!_active)
            {
                throw new InvalidOperationException("You cannot logout before login!");
            }

            RaiseEvent(new LogOutEvent
            {
                id = _id,
                token = token,
                logOutDate = DateTime.UtcNow,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(LogInAggregate),
            });
        }
        public void Apply(LogOutEvent @event)
        {
            _id = @event.id;
        }

        public void EnableSystemNotification(EnableNotificationDTO input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Login first!");
            }

            RaiseEvent(new EnableNotificationEvent
            {
                id = _id,
                messageType = input.messageType,
                enable = input.enable,
                subscription = input.subscription,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(LogInAggregate),
            });
        }
        public void Apply(EnableNotificationEvent @event)
        {
            _id = @event.id;
        }
    }
}

