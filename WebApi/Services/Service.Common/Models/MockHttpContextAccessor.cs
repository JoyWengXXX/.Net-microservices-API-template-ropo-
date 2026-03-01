
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Service.Common.Models
{
    // Mock HttpContextAccessor 用於設定特定的 UserId 作為 EventOperator
    public class MockHttpContextAccessor : IHttpContextAccessor
    {
        private readonly string _userId;

        public MockHttpContextAccessor(string userId)
        {
            _userId = userId;
        }

        public HttpContext? HttpContext 
        { 
            get
            {
                var context = new DefaultHttpContext();
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Name, _userId)
                };
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
                return context;
            }
            set { }
        }
    }
}

