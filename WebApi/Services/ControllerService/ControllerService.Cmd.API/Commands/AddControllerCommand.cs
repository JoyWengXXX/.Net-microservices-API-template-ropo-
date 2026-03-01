using CQRS.Core.Commands;
using ControllerService.Cmd.Domain.DTOs;

namespace ControllerService.Cmd.API.Commands
{
    public class AddControllerCommand : BaseCommand
    {
        public AddControllerDTO input { get; set; }
    }
}

