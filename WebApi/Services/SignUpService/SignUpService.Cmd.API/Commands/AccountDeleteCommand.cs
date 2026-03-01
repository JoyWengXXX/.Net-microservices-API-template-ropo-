using CQRS.Core.Commands;

namespace SignUpService.Cmd.API.Commands
{
    public class AccountDeleteCommand : BaseCommand
    {
        public string userId { get; set; }
    }
}

