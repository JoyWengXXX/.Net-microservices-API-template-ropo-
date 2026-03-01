using DataAccess.Interfaces;
using Npgsql;
using System.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly NpgsqlDataSource _dataSource;
    private NpgsqlConnection _connection;
    private NpgsqlTransaction _transaction;

    public UnitOfWork(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public IDbConnection Connection => _connection ??= _dataSource.CreateConnection();
    public IDbTransaction Transaction => _transaction;

    public void Begin()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            _connection = _dataSource.CreateConnection();
            _connection.Open();
        }

        _transaction = _connection.BeginTransaction();
    }

    public void Commit()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            _transaction.Commit();
        }
        finally
        {
            CleanupTransaction();
        }
    }

    public void Dispose()
    {
        CleanupTransaction();
    }

    public void Rollback()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            CleanupTransaction();
        }
    }

    private void CleanupTransaction()
    {
        if (_transaction != null)
        {
            _transaction.Dispose();
            _transaction = null;
        }

        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }
    }
}
