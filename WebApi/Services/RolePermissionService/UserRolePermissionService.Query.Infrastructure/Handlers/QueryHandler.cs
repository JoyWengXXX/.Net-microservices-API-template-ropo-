using CQRS.Core.Infrastructure;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain.Entities;
using RolePermissionService.Query.Domain.Queries;
using RolePermissionService.Query.Domain.Queries.Interfaces;
using Service.Common.Middleware;

namespace RolePermissionService.Query.Infrastructure.Handlers
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IRepository<MainDBConnectionManager> _repo;

        public QueryHandler(IRepository<MainDBConnectionManager> repo)
        {
            _repo = repo;
        }

        public async Task<TResult> HandleAsync(GetPermissionsQuery query)
        {
            var permissions = await _repo.ComplexQueryAsync<UserRoleBinding>(@"
                    SELECT 
	                    urb.""ControllerId"",
	                    urb.""CreateAllowed"",
	                    urb.""UpdateAllowed"",
	                    urb.""DeleteAllowed"",
	                    urb.""QueryAllowed""
                    FROM 
	                    ""UserRoleBinding"" urb
                    JOIN 
	                    ""StoreMember"" sm ON sm.""StoreId"" = urb.""StoreId"" AND sm.""UserId"" = urb.""UserId""
                    WHERE
	                    urb.""StoreId"" = @storeId
	                    AND urb.""UserId"" = @userId;",
                    new { query.storeId, query.input.userId });
            if (!permissions.Any())
            {
                throw new AppException("Not result found");
            }
            return new TResult { isSuccess = true, executionData = permissions.ToList() };
        }
    }
}

