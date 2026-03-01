using CQRS.Core.Commands;
using SignUpService.Cmd.Domain.DTOs;

namespace SignUpService.Cmd.API.Commands
{
    public class SignInCommand : BaseCommand
    {
        public SignUpDTO input { get; set; }
    }
}

