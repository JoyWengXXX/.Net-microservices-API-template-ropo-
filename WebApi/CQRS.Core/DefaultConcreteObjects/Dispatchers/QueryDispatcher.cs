using CQRS.Core.Infrastructure;
using CQRS.Core.Queries;

namespace CQRS.Core.DefaultConcreteObjects.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly Dictionary<Type, Func<BaseQuery, Task<TResult>>> _handlers = new();

        public void RegisterHandler<TQuery>(Func<TQuery, Task<TResult>> handler) where TQuery : BaseQuery
        {
            if (_handlers.ContainsKey(typeof(TQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register the same query handler twice!");
            }

            _handlers.Add(typeof(TQuery), x => handler((TQuery)x));
        }

        public async Task<TResult> SendAsync(BaseQuery query)
        {
            if (_handlers.TryGetValue(query.GetType(), out Func<BaseQuery, Task<TResult>> handler))
            {
                return await handler(query);
            }

            throw new ArgumentNullException(nameof(handler), "No query handler was registered!");
        }
    }
}

