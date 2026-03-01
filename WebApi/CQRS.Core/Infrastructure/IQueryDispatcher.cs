using CQRS.Core.Queries;

namespace CQRS.Core.Infrastructure
{
    public interface IQueryDispatcher
    {
        void RegisterHandler<TQuery>(Func<TQuery, Task<TResult>> handler) where TQuery : BaseQuery;
        Task<TResult> SendAsync(BaseQuery query);
    }
}

