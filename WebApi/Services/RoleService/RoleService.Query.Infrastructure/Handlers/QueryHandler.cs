using CQRS.Core.Infrastructure;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain.Entities;
using RoleService.Query.Domain.Queries;
using RoleService.Query.Domain.Queries.Interfaces;
using Service.Common.Middleware;
using UserRoleService.Query.Domain.DTOs;

namespace RoleService.Query.Infrastructure.Handlers
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IRepository<MainDBConnectionManager> _repo;

        public QueryHandler(IRepository<MainDBConnectionManager> repo)
        {
            _repo = repo;
        }

        public async Task<TResult> HandleAsync(GetRolesQuery query)
        {
            var roles = await _repo.GetListAsync<UserRole>(x => new { x.RoleId, x.RoleName, x.IsEnable }, x => x.IsEnable == true);
            if (!roles.Any())
            {
                throw new AppException("Roles not found!");
            }
            return new TResult { isSuccess = true, executionData = roles.ToList() };
        }

        public async Task<TResult> HandleAsync(GetRoleInStoreQuery query)
        {
            var role = await _repo.ComplexQueryAsync<ReturnRoleInStoreDTO>(@"
                WITH CTE AS
                (
	                SELECT 
		                ur.""RoleId"", 
                        ur.""RoleOrder""
	                FROM
		                ""StoreMember"" sm
	                JOIN
		                ""UserRoleBinding"" urb ON urb.""UserId"" = sm.""UserId"" AND urb.""StoreId"" = sm.""StoreId""
	                JOIN
		                ""UserRole"" ur ON ur.""RoleId"" = urb.""RoleId""
	                WHERE
		                sm.""UserId"" = @userId AND sm.""StoreId"" = @storeId
                )
                SELECT DISTINCT
	                ur.""RoleOrder"",
	                ur.""RoleId""
                FROM ""UserRole"" ur
                JOIN CTE ON CTE.""RoleId"" != ur.""RoleId""
                WHERE ur.""RoleOrder"" > CTE.""RoleOrder""
                ORDER BY ur.""RoleOrder"";",
                new { query.userId, query.storeId});
            if (role == null) 
            {
                throw new AppException("Roles not found!");
            }
            return new TResult { isSuccess = true, executionData = role };
        }
    }
}

