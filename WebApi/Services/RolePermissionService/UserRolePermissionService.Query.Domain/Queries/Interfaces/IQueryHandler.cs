using CQRS.Core.Infrastructure;

namespace RolePermissionService.Query.Domain.Queries.Interfaces
{
    public interface IQueryHandler
    {
        Task<TResult> HandleAsync(GetPermissionsQuery query);
    }
}

