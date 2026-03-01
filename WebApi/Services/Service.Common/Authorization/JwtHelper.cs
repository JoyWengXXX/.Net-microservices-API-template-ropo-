using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DataAccess;
using DataAccess.Interfaces;
using SystemMain.Entities;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Options;
using Service.Common.Authorization.Interfaces;
using Service.Common.Models;

namespace Service.Common.Authorization
{
    public class JwtHelper : IJwtHelper
    {
        private readonly JwtSettingsOptions _settings;
        private readonly IRepository<MainDBConnectionManager> _repoMain;

        public JwtHelper(IOptionsMonitor<JwtSettingsOptions> settings
                            , IRepository<MainDBConnectionManager> repoMain)
        {
            _settings = settings.CurrentValue;
            _repoMain = repoMain;
        }

        /// <summary>
        /// ?¢ç? JWT Token
        /// </summary>
        /// <param name="tokenInfos"></param>
        /// <returns></returns>
        public string GenerateToken(TokenUserInfo tokenInfos)
        {
            var issuer = _settings.Issuer;
            var signKey = _settings.SignKey;
            var token = JwtBuilder.Create()
                        .WithAlgorithm(new HMACSHA256Algorithm())
                        .WithSecret(signKey)
                        // ä½¿ç”¨æ¨™æ???claim é¡žå?
                        .AddClaim(ClaimTypes.Role, tokenInfos.role)
                        .AddClaim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        .AddClaim(JwtRegisteredClaimNames.Iss, issuer)
                        .AddClaim(JwtRegisteredClaimNames.Sub, tokenInfos.userId)
                        .AddClaim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(_settings.ExpireTimeInHour).ToUnixTimeSeconds())
                        .AddClaim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                        .AddClaim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                        .AddClaim(JwtRegisteredClaimNames.Name, tokenInfos.userId)
                        .AddClaim("photo", tokenInfos.photoUrl)
                        .AddClaim("roleOrder", tokenInfos.roleOrder)
                        .AddClaim("timeZoneId", tokenInfos.timeZoneId)
                        .AddClaim("goodsIsVisibleSwitch", tokenInfos.goodsIsVisibleSwitch)
                        .Encode();
            return token;
        }

        /// <summary>
        /// ?–å? Admin Token
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAdminAuthorization()
        {
            string token = string.Empty;
            var mainUOW = _repoMain.CreateUnitOfWork();
            var adminToken = await _repoMain.GetFirstAsync<LogInRecord>(x => new { x.Token }, x => x.UserId == "TEST@test.com" && x.IsEnable == true, unitOfWork: mainUOW);
            if (adminToken == null)
            {
                token = GenerateToken(new TokenUserInfo()
                {
                    userId = "TEST@test.com",
                    role = "Admin",
                    roleOrder = 0,
                    photoUrl = string.Empty,
                    timeZoneId = "Taipei Standard Time",
                    goodsIsVisibleSwitch = false
                });
                await _repoMain.CreateAsync(new LogInRecord()
                {
                    UserId = "TEST@test.com",
                    Token = token,
                    IsEnable = true, 
                    CreateDate = DateTime.UtcNow,
                }, mainUOW);
            }
            else
            {
                token = adminToken.Token;
            }

            return token;
        }
    }
}
