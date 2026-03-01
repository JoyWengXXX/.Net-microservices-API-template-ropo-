using CQRS.Core.Commands;
using LogInService.Cmd.Domain.DTOs;

namespace LogInService.Cmd.Commands
{
    public class AdminLogInCommand : BaseCommand
    {
        public LogInDTO input { get; set; }
    }
}

