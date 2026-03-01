using CQRS.Core.Queries;
using RolePermissionService.Query.Domain.DTOs;

namespace RolePermissionService.Query.Domain.Queries
{
    public class GetPermissionsQuery : BaseQuery
    {
        public Guid storeId { get; set; }
        public PermissionsQueryDTO input { get; set; }
    }
}

