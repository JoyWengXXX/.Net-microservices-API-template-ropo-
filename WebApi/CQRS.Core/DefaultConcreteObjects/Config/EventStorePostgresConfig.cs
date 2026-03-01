namespace CQRS.Core.DefaultConcreteObjects.Config
{
    public class EventStorePostgresConfig
    {
        public string ConnectionString { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
    }
}

