using CQRS.Core.Events;

namespace ControllerService.Cmd.Domain.Events
{
    public class UpdateControllerEvent : BaseEvent
    {
        public UpdateControllerEvent() : base(nameof(UpdateControllerEvent))
        {
        }

        public string controllerId { get; set; }
        public string controllerName { get; set; }
    }
}

