using static Service.Common.Models.UserInfoEnums;
using CQRS.Core.Infrastructure;
using DataAccess;
using DataAccess.Interfaces;
using SignUpService.Query.Domain.Queries;
using SignUpService.Query.Domain.Queries.Interfaces;
using SignUpService.Query.Domain.DTOs;
using SystemMain.Entities;
using Service.Common.Middleware;
using Service.Common.Models;

namespace SignUpService.Query.Infrastructure.Handlers
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IRepository<MainDBConnectionManager> _repo;

        public QueryHandler(IRepository<MainDBConnectionManager> repo)
        {
            _repo = repo;
        }

        public async Task<TResult> HandleAsync(UserInfoQuery query)
        {
            var userInfo = await _repo.GetFirstAsync<UserInfo>(x => new { x.Name, x.TimeZoneID }, x => x.UserId == query.userId && x.UserStatus == (int)UserStatus.Active);
            if(userInfo == null) { throw new AppException("User not found!", ReturnResultCodeEnums.SystemResultCode.A01); }
            return new TResult
            {
                isSuccess = true,
                executionData = new ReturnUserInfoDTO { name = userInfo.Name, timeZoneId = userInfo.TimeZoneID }
            };
        }

        public async Task<TResult> HandleAsync(AllUserQuery query)
        {
            using var mainUOW = _repo.CreateUnitOfWork();
            var totalPage = await _repo.ComplexQueryAsync<int>(@"SELECT COUNT(*) FROM  ""UserInfo"" WHERE ""IsAdmin"" = FALSE", mainUOW);
            var userInfo = await _repo.ComplexQueryAsync<UserInfo>(@"
                SELECT 
                    ""UserId"",
                    ""Name"",
                    ""UserStatus"",
                    ""IsVerify"",
                    ""SSOType"",
                    ""TimeZoneID""
                FROM 
                    ""UserInfo""
                WHERE 
                    ""IsAdmin"" = FALSE
                ORDER BY
                    ""UserId"" DESC
                LIMIT @PageSize OFFSET @Offset;",
                new
                {
                    query.input.pageSize,
                    Offset = (query.input.pageIndex - 1) * query.input.pageSize
                },
                mainUOW);
            if (userInfo == null) { throw new AppException("User not found!", ReturnResultCodeEnums.SystemResultCode.A01); }
            return new TResult
            {
                isSuccess = true,
                executionData = new ReturnAllUserInfoDTO
                {
                    totalPage = (totalPage.First() % query.input.pageSize) > 0 ? (totalPage.First() / query.input.pageSize) + 1 : (totalPage.First() / query.input.pageSize) < 0 ? 1 : (totalPage.First() / query.input.pageSize),
                    allUsers = userInfo.Select(x => new AllUser
                    {
                        userId = x.UserId,
                        name = x.Name,
                        status = x.UserStatus,
                        isVarified = x.IsVerify,
                        SSOType = x.SSOType,
                        timeZoneId = x.TimeZoneID
                    }).ToList()
                }
            };
        }
    }
}

