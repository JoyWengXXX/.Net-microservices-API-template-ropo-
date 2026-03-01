using DataAccess.Interfaces;
using Npgsql;

namespace DataAccess
{
    public class MainDBConnectionManager : IProjectDBConnectionManager
    {
        private readonly string _connectionString;
        private readonly int _maxPoolSize = 2000, _timeout = 60;

        public MainDBConnectionManager(string ConnectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(ConnectionString)
            {
                MinPoolSize = 10,
                MaxPoolSize = _maxPoolSize,
                Timeout = _timeout,
                CommandTimeout = _timeout,
                Pooling = true
            };
            _connectionString = builder.ToString();
        }

        public string GetConnectionString() => _connectionString;

        public int GetMaxConnectionPool() => _maxPoolSize;

        public int GetTimeout() => _timeout;
    }
}

