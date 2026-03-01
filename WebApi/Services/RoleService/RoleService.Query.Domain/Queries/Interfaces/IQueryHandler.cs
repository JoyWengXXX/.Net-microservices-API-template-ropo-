using CQRS.Core.Infrastructure;
using SystemMain.Entities;

namespace RoleService.Query.Domain.Queries.Interfaces
{
    public interface IQueryHandler
    {
        Task<TResult> HandleAsync(GetRolesQuery query);
        Task<TResult> HandleAsync(GetRoleInStoreQuery query);
    }
}

