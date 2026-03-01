using CQRS.Core.DefaultConcreteObjects.Config;
using SystemMain.Models;
using Microsoft.Extensions.Configuration;
using CommonLibrary.Helpers;

namespace SystemMain
{
    public class DBContextSettingsHelper
    {
        public static DBContextSettings GetSettings()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var DBconfig = ConfigurationHelper.LoadConfiguration(
                directoryInfo.FullName,
                "DBContextAppsettings"
            );
            return new DBContextSettings
            {
                connectionStrings = new ConnectionStrings
                {
                    DefaultConnection = DBconfig.GetValue<string>("ConnectionStrings:DefaultConnection"),
                    EventStoreConnection = DBconfig.GetValue<string>("ConnectionStrings:EventStoreConnection"),
                },
                eventStorePostgresConfig = new EventStorePostgresConfig
                {
                    ConnectionString = DBconfig.GetValue<string>("ConnectionStrings:EventStoreConnection")
                },
                redisWithAOFConnectionConfig = new RedisConnectionConfig
                {
                    ConnectionString = DBconfig.GetValue<string>("RedisConnectionString:RedisWithAOF:ConnectionString"),
                    ExpiryTime = DBconfig.GetValue<int>("RedisConnectionString:RedisWithAOF:ExpiryTime")
                },
                redisWithoutAOFConnectionConfig = new RedisConnectionConfig
                {
                    ConnectionString = DBconfig.GetValue<string>("RedisConnectionString:RedisWithoutAOF:ConnectionString"),
                    ExpiryTime = DBconfig.GetValue<int>("RedisConnectionString:RedisWithoutAOF:ExpiryTime")
                },
                rabbitMQServer = new RabbitMQServer
                {
                    Host = DBconfig.GetValue<string>("RabbitMQServer:Host"),
                    Port = DBconfig.GetValue<int>("RabbitMQServer:Port"),
                    Username = DBconfig.GetValue<string>("RabbitMQServer:Username"),
                    Password = DBconfig.GetValue<string>("RabbitMQServer:Password"),
                    VirtualHost = DBconfig.GetValue<string>("RabbitMQServer:VirtualHost"),
                    AutomaticRecoveryEnabled = DBconfig.GetValue<bool>("RabbitMQServer:AutomaticRecoveryEnabled"),
                    RequestedHeartbeat = DBconfig.GetValue<int>("RabbitMQServer:RequestedHeartbeat"),
                }
            };
        }
    }
}


