using static Service.Common.Models.UserInfoEnums;
using CQRS.Core.Infrastructure;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain;
using SystemMain.Entities;
using LogInService.Cmd.Domain.DTOs;
using LogInService.Cmd.Domain.Handlers;
using LogInService.Cmd.Domain.Models;
using Newtonsoft.Json;
using Service.Common.Middleware;
using Service.Common.Models;
using Service.Common.Models.DTOs;
using StackExchange.Redis;
using System.Linq.Expressions;
using System.Text;
using LogInService.Cmd.Domain.Events;
using Service.Common.Authorization.Interfaces;
using Service.Common.Helpers.Interfaces;
using Microsoft.Extensions.Logging;

namespace LogInService.Cmd.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IRepository<MainDBConnectionManager> _repo;
        private readonly IJwtHelper _jwtHelpers;
        private readonly IConnectionMultiplexer _redisConnection;
        private IHttpClientRequestHelper _httpClientRequestHelper;
        private TimeSpan _expiryTime;
        private readonly ILogger<EventHandler> _logger;

        public EventHandler(IRepository<MainDBConnectionManager> repo,
                            IJwtHelper jwtHelpers,
                            IConnectionMultiplexer redisConnection,
                            IHttpClientRequestHelper httpClientRequestHelper,
                            ILogger<EventHandler> logger)
        {
            _repo = repo;
            _jwtHelpers = jwtHelpers;
            _redisConnection = redisConnection;
            _httpClientRequestHelper = httpClientRequestHelper;
            _logger = logger;

            var DBconfig = DBContextSettingsHelper.GetSettings();
            _expiryTime = TimeSpan.FromHours(DBconfig.redisWithAOFConnectionConfig.ExpiryTime);
        }

        public async Task<TResult> On(LogInEvent @event)
        {
            string userAccount = @event.userAccount;
            string password = @event.password;
            
            using var mainUOW = _repo.CreateUnitOfWork();
            var activeUser = await QueryForUserPermissions(mainUOW, userAccount, password, SSOType.RegularLogin);
            if (activeUser == null)
            {
                throw new AppException("Non user was found!", ReturnResultCodeEnums.SystemResultCode.A01);
            }

            string token = _jwtHelpers.GenerateToken(new TokenUserInfo()
            {
                userId = activeUser.userId,
                roleOrder = activeUser.roleOrder,
                photoUrl = "",
                timeZoneId = activeUser.timeZoneId,
                goodsIsVisibleSwitch = activeUser.goodsIsVisibleSwitch
            }); //產生TOKEN

            //記錄使用者的腳色權限到快取
            var redisDB = _redisConnection.GetDatabase();
            await redisDB.StringSetAsync($"User:{activeUser.userId}:permissions", JsonConvert.SerializeObject(activeUser.userRoleInStores), _expiryTime);
            foreach (var inEachStore in activeUser.permissionCheckBitsInStores)
            {
                await redisDB.StringSetAsync($"User:{activeUser.userId}At:{inEachStore.storeId}:PermissionCheckbits", JsonConvert.SerializeObject(inEachStore.permissionCheckBit), _expiryTime);
            }

            mainUOW.Begin();
            try
            {
                await _repo.UpdateAsync<LogInRecord>(x => x.IsEnable == false, x => x.UserId == activeUser.userId && x.IsEnable == true, mainUOW);
                await _repo.CreateAsync(new LogInRecord
                {
                    UserId = activeUser.userId,
                    Token = token,
                    IsEnable = true,
                    CreateDate = DateTime.UtcNow
                }, mainUOW);

                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult
            {
                isSuccess = true,
                executionData = new LogInResponseDTO
                {
                    token = token
                }
            };
        }

        public async Task<TResult> On(GoogleLogInEvent @event)
        {
            string token = string.Empty;

            using var mainUOW = _repo.CreateUnitOfWork();
            mainUOW.Begin();
            try
            {
                var loginUser = await _repo.GetFirstAsync<UserInfo>(x => new { x.UserId, x.UserStatus }, x => x.UserId == @event.email && x.IsVerify == true, unitOfWork: mainUOW);
                if (loginUser == null)
                {
                    await _repo.CreateAsync(new UserInfo
                    {
                        UserId = @event.email,
                        Password = "",
                        Email = @event.email,
                        Name = @event.name,
                        SSOType = (int)SSOType.Google,
                        Photo = @event.photoUrl,
                        IsVerify = true,
                        TimeZoneID = "Asia/Taipei",
                        CreateDate = DateTime.UtcNow
                    }, mainUOW);
                }
                else if (loginUser.UserStatus == (int)UserStatus.Disabled)
                {
                    throw new AppException("Account disabled", code: ReturnResultCodeEnums.SystemResultCode.A07);
                }

                var activeUser = await QueryForUserPermissions(mainUOW, @event.email, @event.email, SSOType.Google);
                token = _jwtHelpers.GenerateToken(new TokenUserInfo()
                {
                    userId = activeUser.userId,
                    roleOrder = activeUser.roleOrder,
                    photoUrl = @event.photoUrl ?? "",
                    timeZoneId = activeUser.timeZoneId,
                    goodsIsVisibleSwitch = activeUser.goodsIsVisibleSwitch
                });

                //記錄使用者的腳色權限到快取
                var redisDB = _redisConnection.GetDatabase();
                await redisDB.StringSetAsync($"User:{activeUser.userId}:permissions", JsonConvert.SerializeObject(activeUser.userRoleInStores), _expiryTime);
                foreach (var inEachStore in activeUser.permissionCheckBitsInStores)
                {
                    await redisDB.StringSetAsync($"User:{activeUser.userId}At:{inEachStore.storeId}:PermissionCheckbits", JsonConvert.SerializeObject(inEachStore.permissionCheckBit), _expiryTime);
                }

                await _repo.UpdateAsync<LogInRecord>(x => x.IsEnable == false, x => x.UserId == activeUser.userId && x.IsEnable == true, mainUOW);
                await _repo.CreateAsync(new LogInRecord
                {
                    UserId = activeUser.userId,
                    Token = token,
                    IsEnable = true,
                    CreateDate = DateTime.UtcNow
                }, mainUOW);
                
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult
            {
                isSuccess = true,
                executionData = new LogInResponseDTO
                {
                    token = token
                }
            };
        }

        public async Task<TResult> On(AdminLogInEvent @event)
        {
            string userAccount = @event.userAccount;
            string password = @event.password;

            using var mainUOW = _repo.CreateUnitOfWork();
            var activeUser = await QueryForUserPermissions(mainUOW, userAccount, password, SSOType.Admin);
            if (activeUser == null)
            {
                throw new AppException("Non user was found!", ReturnResultCodeEnums.SystemResultCode.A01);
            }

            string token = _jwtHelpers.GenerateToken(new TokenUserInfo()
                {
                    userId = activeUser.userId,
                    roleOrder = activeUser.roleOrder,
                    photoUrl = "",
                    timeZoneId = activeUser.timeZoneId,
                    goodsIsVisibleSwitch = activeUser.goodsIsVisibleSwitch
                }); //產生TOKEN


            //記錄使用者的腳色權限到快取
            var redisDB = _redisConnection.GetDatabase();
            await redisDB.StringSetAsync($"User:{activeUser.userId}:permissions", JsonConvert.SerializeObject(activeUser.userRoleInStores), _expiryTime);
            
            mainUOW.Begin();
            try 
            {
                await _repo.UpdateAsync<LogInRecord>(x => x.IsEnable == false, x => x.UserId == activeUser.userId && x.IsEnable == true, mainUOW);
                await _repo.CreateAsync(new LogInRecord
                {
                    UserId = activeUser.userId,
                    Token = token,
                    IsEnable = true,
                    CreateDate = DateTime.UtcNow
                }, mainUOW);
                mainUOW.Commit();
            }
            catch
            {
                mainUOW.Rollback();
                throw;
            }

            return new TResult
            {
                isSuccess = true,
                executionData = new LogInResponseDTO
                {
                    token = token
                }
            };
        }

        public async Task<TResult> On(LogOutEvent @event)
        {
            Expression<Func<LogInRecord, bool>> input = x => x.IsEnable == false;
            Expression<Func<LogInRecord, bool>> where = x => x.Token == @event.token && x.IsEnable == true;
            await _repo.UpdateAsync(input, where);
            return new TResult { isSuccess = true };
        }

        private async Task<StoreRoleDTO> QueryForUserPermissions(IUnitOfWork mainUOW, string userId, string? password, SSOType loginType)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"SELECT DISTINCT
                                    ui.""UserId"",
                                    ui.""Password"",
                                    ui.""SSOType"",
                                    ui.""IsAdmin"",
                                    ui.""TimeZoneID"",
                                    ui.""GoodsIsVisibleSwitch"",
                                    sm.""StoreId"",
                                    sm.""PermissionCheckBit"",
                                    ur.""RoleId"",
                                    ur.""RoleOrder"",
                                    ur.""RoleName"",
                                    c.""ControllerId"",
                                    urb.""CreateAllowed"",
                                    urb.""UpdateAllowed"",
                                    urb.""DeleteAllowed"",
                                    urb.""QueryAllowed""
                                FROM 
                                    ""UserInfo"" ui
                                LEFT JOIN 
                                    ""StoreMember"" sm ON ui.""UserId"" = sm.""UserId""
                                LEFT JOIN 
                                    ""UserRoleBinding"" urb ON ui.""UserId"" = urb.""UserId"" AND sm.""StoreId"" = urb.""StoreId""
                                LEFT JOIN 
                                    ""UserRole"" ur ON urb.""RoleId"" = ur.""RoleId""
                                LEFT JOIN
                                    ""Controller"" c ON urb.""ControllerId"" = c.""ControllerId"" AND c.""IsEnable"" = TRUE
                                WHERE 
                                    ui.""UserStatus"" = @userStatus
                                    AND ui.""UserId"" = @userId ");

            if(loginType == SSOType.Admin)
            {
                sqlBuilder.AppendLine(@"AND ui.""IsAdmin"" = TRUE;");
            }

            var queryResults = await _repo.ComplexQueryAsync<UserWithPermissions>(sqlBuilder.ToString(), new { userId, userStatus = UserStatus.Active }, mainUOW);
            if(!queryResults.Any())
            {
                throw new AppException("User not found!", ReturnResultCodeEnums.SystemResultCode.A01);
            }
            if(queryResults.Any() && loginType == SSOType.RegularLogin && queryResults.First().Password != password)
            {
                throw new AppException("Wrong userId and password!", ReturnResultCodeEnums.SystemResultCode.A02);
            }

            var userInfo = queryResults.First();
            var storeRoleDTO = new StoreRoleDTO
            {
                userId = userInfo.UserId,
                isAdmin = userInfo.IsAdmin,
                roleOrder = userInfo.RoleOrder,
                timeZoneId = userInfo.TimeZoneID,
                goodsIsVisibleSwitch = userInfo.GoodsIsVisibleSwitch,
                permissionCheckBitsInStores = new List<PermissionCheckBitInStores>(),
                userRoleInStores = new List<UserRoleInStores>()
            };

            foreach (var storeGroup in queryResults.GroupBy(r => r.StoreId))
            {
                var userRoleInStore = new UserRoleInStores
                {
                    storeId = storeGroup.Key,
                    rolePermissions = new List<RolePermissions>()
                };

                var permissionCheckBitInStores = new PermissionCheckBitInStores
                {
                    storeId = storeGroup.Key,
                    permissionCheckBit = storeGroup.First().PermissionCheckBit ?? Array.Empty<byte>()
                };

                foreach (var roleGroup in storeGroup.GroupBy(r => new { RoleID = r.RoleId, r.RoleOrder, r.RoleName }))
                {
                    userRoleInStore.roleId = roleGroup.Key.RoleID;
                    userRoleInStore.roleOrder = roleGroup.Key.RoleOrder;
                    userRoleInStore.roleName = roleGroup.Key.RoleName;

                    foreach (var controllerGroup in roleGroup.GroupBy(r => r.ControllerId))
                    {
                        if (!string.IsNullOrEmpty(controllerGroup.Key))
                        {
                            var rolePermissions = new RolePermissions
                            {
                                controllerId = controllerGroup.Key,
                                permissions = new Dictionary<string, bool>
                                {
                                    { "Create", controllerGroup.Any(x => x.CreateAllowed == true) },
                                    { "Update", controllerGroup.Any(x => x.UpdateAllowed == true) },
                                    { "Delete", controllerGroup.Any(x => x.DeleteAllowed == true) },
                                    { "Query", controllerGroup.Any(x => x.QueryAllowed == true) }
                                }
                            };
                            userRoleInStore.rolePermissions.Add(rolePermissions);
                        }
                    }
                }
                storeRoleDTO.permissionCheckBitsInStores.Add(permissionCheckBitInStores);
                storeRoleDTO.userRoleInStores.Add(userRoleInStore);
            }

            return storeRoleDTO;
        }
    }
}

