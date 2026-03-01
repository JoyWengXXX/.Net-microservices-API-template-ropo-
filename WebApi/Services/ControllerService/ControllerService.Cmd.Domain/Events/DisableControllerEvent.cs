using CQRS.Core.Events;

namespace ControllerService.Cmd.Domain.Events
{
    public class DisableControllerEvent : BaseEvent
    {
        public DisableControllerEvent() : base(nameof(DisableControllerEvent))
        {
        }

        public string controllerId { get; set; }
    }
}

