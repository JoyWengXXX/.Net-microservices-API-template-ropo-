using CQRS.Core.Commands;
using SignUpService.Cmd.Domain.DTOs;

namespace SignUpService.Cmd.API.Commands
{
    public class PasswordChangeCommand : BaseCommand
    {
        public string newPW { get; set; }
    }
}

