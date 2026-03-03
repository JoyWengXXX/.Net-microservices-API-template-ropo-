using CQRS.Core.Infrastructure;

namespace ControllerService.Query.Domain.Queries.Interfaces
{
    public interface IQueryHandler
    {
        Task<TResult> HandleAsync(GetControllersQuery query);
        Task<TResult> HandleAsync(GetControllerByIdQuery query);
    }
}

