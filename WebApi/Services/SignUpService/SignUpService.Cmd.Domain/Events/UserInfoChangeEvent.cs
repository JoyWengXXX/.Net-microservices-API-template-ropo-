using CQRS.Core.Events;
using SignUpService.Cmd.Domain.DTOs;

namespace SignUpService.Cmd.Domain.Events
{
    public class UserInfoChangeEvent : BaseEvent
    {
        public UserInfoChangeEvent() : base(nameof(UserInfoChangeEvent))
        {
        }

        public UpdateUserInfosDTO input { get; set; }
    }
}

