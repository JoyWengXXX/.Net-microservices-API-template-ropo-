namespace CQRS.Core.Events
{
    public class EventModel : EventModelWithoutEventData
    {
        public BaseEvent EventData { get; set; }
    }
    
    public class EventModelWithoutEventData
    {
        public Guid Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public Guid AggregateIdentifier { get; set; }

        public string AggregateType { get; set; }

        public string EventOperator { get; set; }

        public int Version { get; set; }

        public string EventType { get; set; }
    }
}

