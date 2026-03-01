
namespace DataAccess.Interfaces
{
    public interface IProjectDBConnectionManager
    {
        public abstract string GetConnectionString();
        public abstract int GetMaxConnectionPool();
        public abstract int GetTimeout();
    }
}

