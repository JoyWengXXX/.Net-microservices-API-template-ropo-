using CQRS.Core.Commands;
using LogInService.Cmd.Domain.DTOs;

namespace LogInService.Cmd.Commands
{
    public class EnableNotificationCommand : BaseCommand
    {
        public EnableNotificationDTO input { get; set; }
    }
}

