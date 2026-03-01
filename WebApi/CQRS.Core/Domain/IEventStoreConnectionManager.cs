using Npgsql;

namespace CQRS.Core.Domain
{
    public interface IEventStoreConnectionManager
    {
        NpgsqlDataSource GetDataSource();
    }
}

