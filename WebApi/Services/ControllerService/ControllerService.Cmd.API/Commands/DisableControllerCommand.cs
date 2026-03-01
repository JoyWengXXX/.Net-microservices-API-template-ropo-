using CQRS.Core.Commands;

namespace ControllerService.Cmd.API.Commands
{
    public class DisableControllerCommand : BaseCommand
    {
        public string ControllerId { get; set; }
    }
}

