using CQRS.Core.Commands;

namespace LogInService.Cmd.Commands
{
    public class LogOutCommand : BaseCommand
    {
        public string token { get; set; }
    }
}

