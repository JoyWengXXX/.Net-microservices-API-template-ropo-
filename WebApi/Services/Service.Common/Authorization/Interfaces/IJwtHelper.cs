using Service.Common.Models;

namespace Service.Common.Authorization.Interfaces
{
    public interface IJwtHelper
    {
        public string GenerateToken(TokenUserInfo tokenInfos);
        public Task<string> GetAdminAuthorization();
    }
}
