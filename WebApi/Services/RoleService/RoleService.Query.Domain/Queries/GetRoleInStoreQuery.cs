using CQRS.Core.Queries;

namespace RoleService.Query.Domain.Queries
{
    public class GetRoleInStoreQuery : BaseQuery
    {
        public string userId { get; set; }
        public Guid storeId { get; set; }
    }
}

