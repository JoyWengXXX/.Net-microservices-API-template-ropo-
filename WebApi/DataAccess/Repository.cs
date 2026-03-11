using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;
using DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Npgsql;

namespace DataAccess
{
    public class Repository<T1> : IRepository<T1> where T1 : IProjectDBConnectionManager
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ObjectPool<StringBuilder> _stringBuilderPool;
        private readonly SemaphoreSlim _semaphore;
        private readonly ILogger<Repository<T1>> _logger;

        public Repository(T1 dbContext, ILogger<Repository<T1>> logger)
        {
            _dataSource = DatabaseFactory.GetDataSource(dbContext.GetConnectionString());
            _semaphore = new SemaphoreSlim(dbContext.GetMaxConnectionPool(), dbContext.GetMaxConnectionPool());
            _stringBuilderPool = ObjectPool.Create<StringBuilder>();
            _logger = logger;
        }

        public IDbConnection GetConnection()
        {
            var connection = _dataSource.CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork(_dataSource);
        }

        private async Task<int> ExecuteCreateAsync<T2>(T2 input, IUnitOfWork unitOfWork = null) where T2 : class
        {
            var tableAttribute = typeof(T2).GetCustomAttribute<TableAttribute>();
            var properties = typeof(T2).GetProperties();
            var identityColumnName = properties.FirstOrDefault(p => p.GetCustomAttribute<DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)?.Name;
            var sql = _stringBuilderPool.Get();
            sql.Clear();
            sql.Append($"INSERT INTO \"{tableAttribute.Name}\" (");
            var insertColumns = properties.Where(p => p.Name != identityColumnName && p.GetCustomAttribute<NotMappedAttribute>() == null).Select(p => $"\"{p.Name}\"");
            sql.Append(string.Join(",", insertColumns));
            sql.Append(") VALUES (");
            sql.Append(string.Join(",", insertColumns.Select(c => $"@{c.Trim('\"')}")));
            sql.Append(")");

            if (identityColumnName != null)
            {
                sql.Append($" RETURNING \"{identityColumnName}\"");
            }
            string sqlString = sql.ToString();

            await _semaphore.WaitAsync();
            bool isNewConnection = false;
            IDbConnection connection = null;
            IDbTransaction transaction = unitOfWork?.Transaction;
            try
            {
                if (unitOfWork?.Connection != null)
                {
                    connection = unitOfWork.Connection;
                }
                else
                {
                    connection = GetConnection();
                    isNewConnection = true;
                }

                return await ExecuteWithRetryAsync(async () =>
                {
                    return await connection.ExecuteScalarAsync<int>(sqlString, input, transaction: transaction).ConfigureAwait(false);
                });
            }
            finally
            {
                if (isNewConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
                _semaphore.Release();
                _stringBuilderPool.Return(sql);
            }
        }

        private async Task<int> ExecuteUpdateAsync<T2>(Expression<Func<T2, bool>> input, Expression<Func<T2, bool>> predicate, IUnitOfWork unitOfWork = null) where T2 : class
        {
            var tableAttribute = typeof(T2).GetCustomAttribute<TableAttribute>();
            var sql = _stringBuilderPool.Get();
            sql.Clear();
            sql.Append($"UPDATE \"{tableAttribute.Name}\"");
            sql.Append(" SET ");
            var inputVisitor = new UpdateExpressionVisitor();
            inputVisitor.Visit(input);
            sql.Append(inputVisitor.Sql);
            var visitor = new WhereExpressionVisitor();
            visitor.Visit(predicate);
            sql.Append(" WHERE ");
            sql.Append(visitor.Sql);
            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(inputVisitor.Parameters);
            foreach (var item in visitor.Parameters.ParameterNames)
            {
                if (!inputVisitor.Parameters.ParameterNames.Contains(item))
                    parameters.Add(item, visitor.Parameters.Get<object>(item));
            }
            string sqlString = sql.ToString();

            await _semaphore.WaitAsync();
            bool isNewConnection = false;
            IDbConnection connection = null;
            IDbTransaction transaction = unitOfWork?.Transaction;
            try
            {
                if (unitOfWork?.Connection != null)
                {
                    connection = unitOfWork.Connection;
                }
                else
                {
                    connection = GetConnection();
                    isNewConnection = true;
                }

                return await ExecuteWithRetryAsync(async () =>
                {
                    return await connection.ExecuteAsync(sqlString, parameters, transaction: transaction).ConfigureAwait(false);
                });
            }
            finally
            {
                if (isNewConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
                _semaphore.Release();
                _stringBuilderPool.Return(sql);
            }
        }

        private async Task<int> ExecuteDeleteAsync<T2>(Expression<Func<T2, bool>> predicate, IUnitOfWork unitOfWork = null) where T2 : class
        {
            var tableAttribute = typeof(T2).GetCustomAttribute<TableAttribute>();
            var sql = _stringBuilderPool.Get();
            sql.Clear();
            sql.Append($"DELETE FROM \"{tableAttribute.Name}\" ");

            await _semaphore.WaitAsync();
            bool isNewConnection = false;
            IDbConnection connection = null;
            IDbTransaction transaction = unitOfWork?.Transaction;
            try
            {
                if (unitOfWork?.Connection != null)
                {
                    connection = unitOfWork.Connection;
                }
                else
                {
                    connection = GetConnection();
                    isNewConnection = true;
                }

                if (predicate != null)
                {
                    var visitor = new WhereExpressionVisitor();
                    visitor.Visit(predicate);
                    sql.Append("WHERE ");
                    sql.Append(visitor.Sql);
                    var parameters = visitor.Parameters;
                    string sqlString = sql.ToString();

                    return await ExecuteWithRetryAsync(async () =>
                    {
                        return await connection.ExecuteAsync(sqlString, parameters, transaction: transaction).ConfigureAwait(false);
                    });
                }
                else
                {
                    string sqlString = sql.ToString();
                    return await ExecuteWithRetryAsync(async () =>
                    {
                        return await connection.ExecuteAsync(sqlString, transaction: transaction).ConfigureAwait(false);
                    });
                }
            }
            finally
            {
                if (isNewConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
                _semaphore.Release();
                _stringBuilderPool.Return(sql);
            }
        }

        private async Task<int> ExecuteComplexCommandAsync(string sqlStr, object parameters = null, IUnitOfWork unitOfWork = null)
        {
            await _semaphore.WaitAsync();
            bool isNewConnection = false;
            IDbConnection connection = null;
            IDbTransaction transaction = unitOfWork?.Transaction;
            try
            {
                if (unitOfWork?.Connection != null)
                {
                    connection = unitOfWork.Connection;
                }
                else
                {
                    connection = GetConnection();
                    isNewConnection = true;
                }

                return await ExecuteWithRetryAsync(async () =>
                {
                    return await connection.ExecuteAsync(sqlStr, parameters, transaction: transaction).ConfigureAwait(false);
                });
            }
            finally
            {
                if (isNewConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
                _semaphore.Release();
            }
        }

        private async Task<IEnumerable<T2>?> ExecuteGetListAsync<T2>(Expression<Func<T2, object>> selected, Expression<Func<T2, bool>>? predicate = null, Expression<Func<List<T2>, object>>? orderBy = null, IUnitOfWork unitOfWork = null) where T2 : class
        {
            var tableAttribute = typeof(T2).GetCustomAttribute<TableAttribute>();
            var sql = _stringBuilderPool.Get();
            sql.Clear();
            var inputVisitor = new SelectExpressionVisitor();
            inputVisitor.Visit(selected);
            inputVisitor.BuildSelectClause();
            sql.Append($"SELECT {inputVisitor.Sql} ");
            sql.Append($"FROM \"{tableAttribute.Name}\" ");

            var visitor = new WhereExpressionVisitor();
            visitor.Visit(predicate);
            if (predicate != null && !string.IsNullOrEmpty(visitor.Sql))
                sql.Append("WHERE ");
            sql.Append(visitor.Sql);

            // Append ORDER BY clause.
            if (orderBy != null)
            {
                var orderByVisitor = new OrderByExpressionVisitor();
                orderByVisitor.Visit(orderBy);
                sql.Append($" ORDER BY {orderByVisitor.Sql} ");
            }

            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(inputVisitor.Parameters);
            foreach (var item in visitor.Parameters.ParameterNames)
            {
                if (!inputVisitor.Parameters.ParameterNames.Contains(item))
                    parameters.Add(item, visitor.Parameters.Get<object>(item));
            }
            string sqlString = sql.ToString();

            await _semaphore.WaitAsync();
            bool isNewConnection = false;
            IDbConnection connection = null;
            IDbTransaction transaction = unitOfWork?.Transaction;
            try
            {
                if (unitOfWork?.Connection != null)
                {
                    connection = unitOfWork.Connection;
                }
                else
                {
                    connection = GetConnection();
                    isNewConnection = true;
                }

                return await ExecuteWithRetryAsync(async () =>
                {
                    return await connection.QueryAsync<T2>(sqlString, parameters, transaction: transaction).ConfigureAwait(false);
                });
            }
            finally
            {
                if (isNewConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
                _semaphore.Release();
                _stringBuilderPool.Return(sql);
            }
        }

        private async Task<T2?> ExecuteGetFirstAsync<T2>(Expression<Func<T2, object>> selected, Expression<Func<T2, bool>>? predicate = null, Expression<Func<List<T2>, object>>? orderBy = null, IUnitOfWork unitOfWork = null) where T2 : class
        {
            var tableAttribute = typeof(T2).GetCustomAttribute<TableAttribute>();
            var sql = _stringBuilderPool.Get();
            sql.Clear();
            var inputVisitor = new SelectExpressionVisitor();
            inputVisitor.Visit(selected);
            inputVisitor.BuildSelectClause();
            sql.Append($"SELECT {inputVisitor.Sql} ");

            sql.Append($"FROM \"{tableAttribute.Name}\" ");

            var visitor = new WhereExpressionVisitor();
            visitor.Visit(predicate);
            if (predicate != null)
                sql.Append("WHERE ");
            sql.Append(visitor.Sql);

            // Append ORDER BY clause.
            if (orderBy != null)
            {
                var orderByVisitor = new OrderByExpressionVisitor();
                orderByVisitor.Visit(orderBy);
                sql.Append($" ORDER BY {orderByVisitor.Sql} ");
            }

            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(inputVisitor.Parameters);
            foreach (var item in visitor.Parameters.ParameterNames)
            {
                if (!inputVisitor.Parameters.ParameterNames.Contains(item))
                    parameters.Add(item, visitor.Parameters.Get<object>(item));
            }
            string sqlString = sql.ToString();

            await _semaphore.WaitAsync();
            bool isNewConnection = false;
            IDbConnection connection = null;
            IDbTransaction transaction = unitOfWork?.Transaction;
            try
            {
                if (unitOfWork?.Connection != null)
                {
                    connection = unitOfWork.Connection;
                }
                else
                {
                    connection = GetConnection();
                    isNewConnection = true;
                }

                return await ExecuteWithRetryAsync(async () =>
                {
                    return await connection.QueryFirstOrDefaultAsync<T2>(sqlString, parameters, transaction: transaction).ConfigureAwait(false);
                });
            }
            finally
            {
                if (isNewConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
                _semaphore.Release();
                _stringBuilderPool.Return(sql);
            }
        }

        private async Task<IEnumerable<T2>?> ExecuteComplexQueryAsync<T2>(string sqlStr, object parameters = null, IUnitOfWork unitOfWork = null)
        {
            await _semaphore.WaitAsync();
            bool isNewConnection = false;
            IDbConnection connection = null;
            IDbTransaction transaction = unitOfWork?.Transaction;
            try
            {
                if (unitOfWork?.Connection != null)
                {
                    connection = unitOfWork.Connection;
                }
                else
                {
                    connection = GetConnection();
                    isNewConnection = true;
                }

                return await ExecuteWithRetryAsync(async () =>
                {
                    return await connection.QueryAsync<T2>(sqlStr, parameters, transaction: transaction).ConfigureAwait(false);
                });
            }
            finally
            {
                if (isNewConnection && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
                _semaphore.Release();
            }
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
        {
            int retryCount = 0;
            const int maxRetries = 5;
            while (true)
            {
                try
                {
                    return await operation();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"SQL executed error: {ex.Message}");
                    _logger.LogError($"SQL executed error: {ex.Message}");
                    if (retryCount >= maxRetries)
                    {
                        throw;
                    }
                    retryCount++;
                    await Task.Delay(100 * retryCount);
                }
            }
        }

        public Task<IEnumerable<T2>?> GetListAsync<T2>(Expression<Func<T2, object>> selected, Expression<Func<T2, bool>>? predicate = null, Expression<Func<List<T2>, object>>? orderBy = null, IUnitOfWork unitOfWork = null) where T2 : class
        {
            return ExecuteGetListAsync(selected, predicate, orderBy, unitOfWork);
        }

        public Task<T2?> GetFirstAsync<T2>(Expression<Func<T2, object>> selected, Expression<Func<T2, bool>>? predicate = null, Expression<Func<List<T2>, object>>? orderBy = null, IUnitOfWork? unitOfWork = null) where T2 : class
        {
            return ExecuteGetFirstAsync(selected, predicate, orderBy, unitOfWork);
        }

        public Task<IEnumerable<T2>?> ComplexQueryAsync<T2>(string sqlStr, object parameters = null, IUnitOfWork unitOfWork = null)
        {
            return ExecuteComplexQueryAsync<T2>(sqlStr, parameters, unitOfWork);
        }

        public Task<int> CreateAsync<T2>(T2 input, IUnitOfWork unitOfWork = null) where T2 : class
        {
            return ExecuteCreateAsync(input, unitOfWork);
        }

        public Task<int> UpdateAsync<T2>(Expression<Func<T2, bool>> input, Expression<Func<T2, bool>> predicate = null, IUnitOfWork unitOfWork = null) where T2 : class
        {
            return ExecuteUpdateAsync(input, predicate, unitOfWork);
        }

        public Task<int> DeleteAsync<T2>(Expression<Func<T2, bool>> predicate = null, IUnitOfWork unitOfWork = null) where T2 : class
        {
            return ExecuteDeleteAsync(predicate, unitOfWork);
        }

        public Task<int> ComplexCommandAsync(string sqlStr, object parameters = null, IUnitOfWork unitOfWork = null)
        {
            return ExecuteComplexCommandAsync(sqlStr, parameters, unitOfWork);
        }


        /// <summary>
        /// Builds SQL for SELECT clause.
        /// </summary>
        private class SelectExpressionVisitor : ExpressionVisitor
        {
            private readonly DynamicParameters _parameters = new DynamicParameters();
            private List<string> _selectedColumns = new List<string>();

            public string Sql { get; private set; } = string.Empty;
            public DynamicParameters Parameters => _parameters;

            protected override Expression VisitMember(MemberExpression node)
            {
                var columnName = node.Member.Name;
                _selectedColumns.Add($"\"{columnName}\"");

                return base.VisitMember(node);
            }

            public void BuildSelectClause()
            {
                Sql = string.Join(", ", _selectedColumns);
            }
        }
        /// <summary>
        /// Builds SQL for UPDATE clause.
        /// </summary>
        private class UpdateExpressionVisitor : ExpressionVisitor
        {
            private readonly DynamicParameters _parameters = new DynamicParameters();

            public string Sql { get; private set; } = string.Empty;
            public DynamicParameters Parameters => _parameters;

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.Equal)
                {
                    if (node.Left is MemberExpression left)
                    {
                        var leftMemberName = left.Member.Name;
                        var rightValue = Expression.Lambda(node.Right).Compile().DynamicInvoke(); // Compile right expression to get parameter value.
                        var paramName = $"@{left.Member.Name}";
                        _parameters.Add(paramName, rightValue); // Add constant value as a Dapper parameter.

                        Sql += $"\"{leftMemberName}\" = {paramName}";
                    }
                    else if (node.Left is ConstantExpression constant)
                    {
                        var leftMemberName = constant.Value.ToString();
                        var rightValue = Expression.Lambda(node.Right).Compile().DynamicInvoke(); // Compile right expression to get parameter value.
                        var paramName = $"@{constant.Value}";
                        _parameters.Add(paramName, rightValue); // Add constant value as a Dapper parameter.

                        Sql += $"\"{leftMemberName}\" = {paramName}";
                    }
                    else if (node.Left is UnaryExpression unary)
                    {
                        var leftMemberName = ((MemberExpression)unary.Operand).Member.Name;
                        var rightValue = Expression.Lambda(node.Right).Compile().DynamicInvoke(); // Compile right expression to get parameter value.
                        var paramName = $"@{leftMemberName}";
                        _parameters.Add(paramName, rightValue); // Add constant value as a Dapper parameter.

                        Sql += $"\"{leftMemberName}\" = {paramName}";
                    }
                }
                else if (node.NodeType == ExpressionType.AndAlso)
                {
                    Visit(node.Left);
                    Sql += " , ";
                    Visit(node.Right);
                }
                return node;
            }
        }
        /// <summary>
        /// Builds SQL for WHERE clause.
        /// </summary>
        private class WhereExpressionVisitor : ExpressionVisitor
        {
            private readonly DynamicParameters _parameters = new DynamicParameters();

            public string Sql { get; private set; } = string.Empty;
            public int parameterCount { get; private set; } = 1;
            public DynamicParameters Parameters => _parameters;

            /// <summary>
            /// Handles unary expressions, including NOT Contains.
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Not && node.Operand is MethodCallExpression methodCall)
                {
                    if (methodCall.Method.Name == "Contains")
                    {
                        VisitMethodCall(methodCall, true);
                        return node;
                    }
                }
                else if (node.NodeType == ExpressionType.Not)
                {
                    if (node.Operand is MemberExpression memberExpression)
                    {
                        var columnName = memberExpression.Member.Name;
                        Sql += $"\"{columnName}\" = FALSE";
                        return node;
                    }
                }
                return base.VisitUnary(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Contains")
                {
                    VisitMethodCall(node, false);
                }
                return base.VisitMethodCall(node);
            }

            private void VisitMethodCall(MethodCallExpression node, bool isNotContains)
            {
                Expression collection;
                Expression item;
                if (node.Object != null)
                {
                    // Handle list.Contains(item) case.
                    collection = node.Object;
                    item = node.Arguments[0];
                }
                else
                {
                    // Handle Enumerable.Contains(list, item) case.
                    collection = node.Arguments[0];
                    item = node.Arguments[1];
                }

                if (item is MemberExpression memberExpression)
                {
                    var columnName = memberExpression.Member.Name;
                    var values = Expression.Lambda(collection).Compile().DynamicInvoke() as IEnumerable;

                    if (values != null)
                    {
                        var inClauseValues = new List<string>();
                        foreach (var value in values)
                        {
                            var paramName = $"@{columnName}{parameterCount}";
                            _parameters.Add(paramName, value);
                            inClauseValues.Add(paramName);
                            parameterCount++;
                        }

                        string operation = isNotContains ? "NOT IN" : "IN";
                        Sql += $"\"{columnName}\" {operation} ({string.Join(", ", inClauseValues)})";
                    }
                }
            }

            /// <summary>
            /// Handles other expression cases.
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.AndAlso)
                {
                    Visit(node.Left);
                    Sql += " AND ";
                    Visit(node.Right);
                }
                else if (node.NodeType == ExpressionType.OrElse)
                {
                    Visit(node.Left);
                    Sql += " OR ";
                    Visit(node.Right);
                }
                else
                {
                    if (node.Left is MemberExpression left)
                    {
                        var leftMemberName = left.Member.Name;
                        var paramName = $"@{left.Member.Name}{parameterCount}";
                        var rightValue = Expression.Lambda(node.Right).Compile().DynamicInvoke();
                        _parameters.Add(paramName, rightValue);

                        switch (node.NodeType)
                        {
                            case ExpressionType.Equal:
                                Sql += $"\"{leftMemberName}\" = {paramName}";
                                break;
                            case ExpressionType.NotEqual:
                                Sql += $"\"{leftMemberName}\" <> {paramName}";
                                break;
                            case ExpressionType.GreaterThan:
                                Sql += $"\"{leftMemberName}\" > {paramName}";
                                break;
                            case ExpressionType.GreaterThanOrEqual:
                                Sql += $"\"{leftMemberName}\" >= {paramName}";
                                break;
                            case ExpressionType.LessThan:
                                Sql += $"\"{leftMemberName}\" < {paramName}";
                                break;
                            case ExpressionType.LessThanOrEqual:
                                Sql += $"\"{leftMemberName}\" <= {paramName}";
                                break;
                        }
                    }
                    else if (node.Left is ConstantExpression constant)
                    {
                        var leftMemberName = constant.Value.ToString();
                        var rightValue = Expression.Lambda(node.Right).Compile().DynamicInvoke();
                        var paramName = $"@{constant.Value}";
                        _parameters.Add(paramName, rightValue);

                        string operatorString = node.NodeType switch
                        {
                            ExpressionType.Equal => "=",
                            ExpressionType.NotEqual => "<>",
                            ExpressionType.GreaterThan => ">",
                            ExpressionType.GreaterThanOrEqual => ">=",
                            ExpressionType.LessThan => "<",
                            ExpressionType.LessThanOrEqual => "<=",
                            _ => "="
                        };

                        Sql += $"\"{leftMemberName}\" {operatorString} {paramName}";
                    }
                    else if (node.Left is UnaryExpression unary)
                    {
                        var leftMemberName = ((MemberExpression)unary.Operand).Member.Name;
                        var rightValue = Expression.Lambda(node.Right).Compile().DynamicInvoke();
                        var paramName = $"@{leftMemberName}";
                        _parameters.Add(paramName, rightValue);

                        string operatorString = node.NodeType switch
                        {
                            ExpressionType.Equal => "=",
                            ExpressionType.NotEqual => "<>",
                            ExpressionType.GreaterThan => ">",
                            ExpressionType.GreaterThanOrEqual => ">=",
                            ExpressionType.LessThan => "<",
                            ExpressionType.LessThanOrEqual => "<=",
                            _ => "="
                        };

                        Sql += $"\"{leftMemberName}\" {operatorString} {paramName}";
                    }
                }
                parameterCount++;
                return node;
            }
        }
        /// <summary>
        /// Builds SQL for ORDER BY clause.
        /// </summary>
        public class OrderByExpressionVisitor : ExpressionVisitor
        {
            private List<string> _orderByComponents = new List<string>();

            public string Sql => string.Join(", ", _orderByComponents);

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "OrderBy" || node.Method.Name == "OrderByDescending" ||
                    node.Method.Name == "ThenBy" || node.Method.Name == "ThenByDescending")
                {
                    var isDescending = node.Method.Name.EndsWith("Descending");

                    // Handle lambda argument.
                    var lambdaExpression = node.Arguments[1];
                    if (lambdaExpression is UnaryExpression unaryExpression)
                    {
                        lambdaExpression = unaryExpression.Operand;
                    }

                    if (lambdaExpression is LambdaExpression lambda)
                    {
                        var memberExpression = lambda.Body as MemberExpression;
                        if (memberExpression != null)
                        {
                            var orderByClause = $"\"{memberExpression.Member.Name}\"";
                            if (isDescending)
                            {
                                orderByClause += " DESC";
                            }
                            _orderByComponents.Insert(0, orderByClause);
                        }
                    }

                    // Handle chained order-by methods.
                    if (node.Arguments[0] is MethodCallExpression innerMethod)
                    {
                        VisitMethodCall(innerMethod);
                    }
                }
                else
                {
                    throw new NotSupportedException($"The method '{node.Method.Name}' is not supported in OrderBy clause.");
                }

                return node;
            }

            public void BuildOrderByClause(Expression expression)
            {
                _orderByComponents.Clear();
                Visit(expression);
            }
        }
    }
}
