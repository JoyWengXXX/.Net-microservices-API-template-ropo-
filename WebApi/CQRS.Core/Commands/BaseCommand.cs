using CQRS.Core.Messages;

namespace CQRS.Core.Commands
{
    public abstract class BaseCommand : Message
    {
        public DateTime createOn { get; set; }
    }
}

