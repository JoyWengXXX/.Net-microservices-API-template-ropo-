using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Service.Common.Middleware;
using Service.Common.Models.DTOs;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;

namespace Service.Common.Filters
{
    /// <summary>
    /// 檢查API動作是否有符合各店家中設定角色權限
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ActionRoleFilter : Attribute, IAsyncActionFilter
    {
        /// <summary>
        /// 建立 ActionRoleFilter
        /// </summary>
        public ActionRoleFilter()
        {
            
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 從 DI 獲取 Redis 連接
            var redisConnection = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

            // 取得 HttpContext
            var httpContext = context.HttpContext;
            // 檢查是否存在 Authorization 標頭
            if (httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                // 取得 Authorization 標頭的值
                var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();
                // 檢查是否以 "Bearer " 開頭，表示是 JWT Token
                if (authorizationHeader.StartsWith("Bearer "))
                {
                    // 提取 Token
                    var token = authorizationHeader.Substring(7);
                    // 解析 Token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var JWTToken = tokenHandler.ReadJwtToken(token);

                    // 取得所需的資訊
                    var controller = context.ActionDescriptor.RouteValues["controller"];
                    List<RolePermissions> activatedPermissions = new List<RolePermissions>();
                    string userId = JWTToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
                    var redisDB = redisConnection.GetDatabase();
                    var userPermissions = await redisDB.StringGetAsync($"User:{userId}:permissions");
                    if (!userPermissions.HasValue)
                    {
                        throw new AppException("Permissions not found!");
                    }
                    var permissionJSON = JsonConvert.DeserializeObject<List<UserRoleInStores>>(userPermissions.ToString());
                    RedisValue storeId = Guid.Empty.ToString();

                    await next();
                }
            }
        }
    }
}
