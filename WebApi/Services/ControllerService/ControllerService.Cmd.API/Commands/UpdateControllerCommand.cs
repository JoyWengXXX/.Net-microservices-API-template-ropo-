using CQRS.Core.Commands;
using ControllerService.Cmd.Domain.DTOs;

namespace ControllerService.Cmd.API.Commands
{
    public class UpdateControllerCommand : BaseCommand
    {
        public UpdateControllerDTO input { get; set; }
    }
}

