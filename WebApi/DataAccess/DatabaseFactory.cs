using Npgsql;
using System.Collections.Concurrent;

public class DatabaseFactory : IDisposable
{
    private static readonly ConcurrentDictionary<string, NpgsqlDataSource> _dataSources = new();
    private bool _disposed;

    public static NpgsqlDataSource GetDataSource(string connectionString)
    {
        return _dataSources.GetOrAdd(connectionString, connStr =>
        {
            var builder = new NpgsqlDataSourceBuilder(connStr);
            return builder.Build();
        });
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var dataSource in _dataSources.Values)
                {
                    dataSource.Dispose();
                }
                _dataSources.Clear();
            }
            _disposed = true;
        }
    }
}
