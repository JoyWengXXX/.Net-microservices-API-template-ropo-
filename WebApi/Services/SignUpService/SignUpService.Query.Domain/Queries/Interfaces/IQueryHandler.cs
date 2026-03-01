using CQRS.Core.Infrastructure;

namespace SignUpService.Query.Domain.Queries.Interfaces
{
    public interface IQueryHandler
    {
        Task<TResult> HandleAsync(UserInfoQuery query);
        Task<TResult> HandleAsync(AllUserQuery query);
    }
}

