using CQRS.Core.Domain;
using SignUpService.Cmd.Domain.Events;
using Microsoft.AspNetCore.Http;
using SignUpService.Cmd.Domain.DTOs;
using SignUpService.Cmd.Domain.Models;

namespace SignUpService.Cmd.Domain.Aggregates
{
    public class SignUpAggregate : AggregateRoot
    {
        private bool _active;

        public bool Active { get => _active; set => _active = value; }

        public SignUpAggregate(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }
        public SignUpAggregate(IHttpContextAccessor httpContextAccessor, Guid messageId, SignUpDTO input) : base(httpContextAccessor)
        {
            RaiseEvent(new SignUpEvent
            {
                id = messageId,
                userId = input.userId,
                password = input.password,
                name = input.name,
                Type = nameof(SignUpAggregate),
            });
        }
        public void Apply(SignUpEvent @event)
        {
            _id = @event.id;
            _active = true;
        }

        public void Validate(ValidateParameters input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new ValidateEvent
            {
                id = _id,
                userId = input.userId,
                validationCode = input.validationCode,
                Type = nameof(SignUpAggregate),
            });
        }
        public void Apply(ValidateEvent @event)
        {
            _id = @event.id;
        }

        public void ForgetPassword(ForgetPasswordDTO input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new ForgetPasswordEvent
            {
                id = _id,
                userId = input.userId,
                Type = nameof(SignUpAggregate),
            });
        }
        public void Apply(ForgetPasswordEvent @event)
        {
            _id = @event.id;
        }

        public void PasswordChange(string input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new PasswordChangeEvent
            {
                id = _id,
                newPassword = input,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(SignUpAggregate)
            });
        }
        public void Apply(PasswordChangeEvent @event)
        {
            _id = @event.id;
        }

        public void UserInfoChange(UpdateUserInfosDTO input)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new UserInfoChangeEvent
            {
                id = _id,
                input = input,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(SignUpAggregate)
            });
        }
        public void Apply(UserInfoChangeEvent @event)
        {
            _id = @event.id;
        }

        public void AccountDisable(string userId)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new AccountDisableEvent
            {
                id = _id,
                userId = userId,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(SignUpAggregate)
            });
        }
        public void Apply(AccountDisableEvent @event)
        {
            _id = @event.id;
        }

        public void AccountDelete(string userId)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Please sign in first!");
            }

            RaiseEvent(new AccountDeleteEvent
            {
                id = _id,
                userId = userId,
                EventOperator = GetCurrentOperatorId(),
                Type = nameof(SignUpAggregate)
            });
        }
        public void Apply(AccountDeleteEvent @event)
        {
            _id = @event.id;
        }
    }
}

