using CQRS.Core.Commands;

namespace SignUpService.Cmd.API.Commands
{
    public class AccountDisableCommand : BaseCommand
    {
        public string userId { get; set; }
    }
}

