using System;
using System.Text.Json.Nodes;

namespace SystemMain.Entities
{
    public class EventSourcingEvent
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public Guid AggregateIdentifier { get; set; }
        public string AggregateType { get; set; }
        public string EventOperator { get; set; }
        public int Version { get; set; }
        public string EventType { get; set; }
        public JsonNode EventData { get; set; }
    }
}


