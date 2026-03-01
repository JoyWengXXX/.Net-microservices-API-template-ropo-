using CQRS.Core.Messages;

namespace CQRS.Core.Events
{
    public class BaseEvent : Message
    {
        protected BaseEvent(string type)
        {
            Type = type;
        }

        public string EventOperator { get; set; }
        public string Type { get; set; }
        public string? InputJson { get; set; }
        public int Version { get; set; }
    }
}

