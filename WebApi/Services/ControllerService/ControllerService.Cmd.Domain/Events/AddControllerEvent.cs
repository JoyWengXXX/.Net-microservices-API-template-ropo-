using CQRS.Core.Events;

namespace ControllerService.Cmd.Domain.Events
{
    public class AddControllerEvent : BaseEvent
    {
        public AddControllerEvent() : base(nameof(AddControllerEvent))
        {
        }

        public string controllerId { get; set; }
        public string controllerName { get; set; }
    }
}

