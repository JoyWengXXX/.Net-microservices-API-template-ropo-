using CQRS.Core.Commands;
using SignUpService.Cmd.Domain.Models;

namespace SignUpService.Cmd.API.Commands
{
    public class ValidateCommand : BaseCommand
    {
        public ValidateParameters input { get; set; }
    }
}

