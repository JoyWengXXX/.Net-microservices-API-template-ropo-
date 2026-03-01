using CQRS.Core.DefaultConcreteObjects.Config;
using CQRS.Core.Domain;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CQRS.Core.DefaultConcreteObjects.Repository
{
    public class EventStorePostgresConnectionManager : IEventStoreConnectionManager, IDisposable
    {
        private readonly NpgsqlDataSource _dataSource;
        private bool _disposed;

        public EventStorePostgresConnectionManager(IOptions<EventStorePostgresConfig> config)
        {
            var builder = new NpgsqlDataSourceBuilder(config.Value.ConnectionString);
            _dataSource = builder.Build();
        }

        public NpgsqlDataSource GetDataSource() => _dataSource;

        public void Dispose()
        {
            if (!_disposed)
            {
                _dataSource.Dispose();
                _disposed = true;
            }
        }
    }
}

