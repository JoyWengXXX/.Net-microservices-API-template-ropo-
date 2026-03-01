using CQRS.Core.Commands;
using SignUpService.Cmd.Domain.DTOs;

namespace SignUpService.Cmd.API.Commands
{
    public class ForgetPasswordCommand : BaseCommand
    {
        public ForgetPasswordDTO input { get; set; }
    }
}

