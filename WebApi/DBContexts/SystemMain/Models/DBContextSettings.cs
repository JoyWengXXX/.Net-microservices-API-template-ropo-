using CQRS.Core.DefaultConcreteObjects.Config;

namespace SystemMain.Models
{
    public class DBContextSettings
    {
        public ConnectionStrings connectionStrings {  get; set; }
        public RedisConnectionConfig redisWithAOFConnectionConfig {  get; set; }
        public RedisConnectionConfig redisWithoutAOFConnectionConfig {  get; set; }
        public EventStorePostgresConfig eventStorePostgresConfig {  get; set; }
        public RabbitMQServer rabbitMQServer {  get; set; }
    }

    public class ConnectionStrings()
    {
        public string DefaultConnection { get; set; }
        public string EventStoreConnection { get; set; }
        public string MeterValuesConnection { get; set; }
        public string AdminConnection { get; set; }
    }

    public class RedisConnectionConfig()
    {
        public string ConnectionString { get; set; }
        public int ExpiryTime { get; set; }
        public int EgmInfoRedisDbIdx { get; set; }
    }

    public class RabbitMQServer()
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public bool AutomaticRecoveryEnabled { get; set; }
        public int RequestedHeartbeat { get; set; }
    }
}


