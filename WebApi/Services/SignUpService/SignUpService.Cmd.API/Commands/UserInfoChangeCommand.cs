using CQRS.Core.Commands;
using SignUpService.Cmd.Domain.DTOs;

namespace SignUpService.Cmd.API.Commands
{
    public class UserInfoChangeCommand : BaseCommand
    {
        public UpdateUserInfosDTO input { get; set; }
    }
}

