using CQRS.Core.Commands;
using LogInService.Cmd.Domain.Models;

namespace LogInService.Cmd.Commands
{
    public class GoogleLogInCommand : BaseCommand
    {
        public GoogleCredentialPayload input {  get; set; }
    }
}

