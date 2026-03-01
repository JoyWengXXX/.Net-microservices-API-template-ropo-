using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using CommonLibrary.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service.Common.Authorization
{
    public static class JWTAuthorizationSetting
    {
        public static void JWTSetting(this WebApplicationBuilder builder)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            //DbContext injection
            var configuration = ConfigurationHelper.LoadConfiguration(
                directoryInfo.FullName,
                "CommonSettings"
            );

            builder.Services.Configure<JwtSettingsOptions>(configuration.GetSection("JWTSettings"));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.IncludeErrorDetails = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration.GetValue<string>("JWTSettings:Issuer"),
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JWTSettings:SignKey"))),
                        // 使用標準的 claim 類型
                        NameClaimType = JwtRegisteredClaimNames.Sub,
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            builder.Services.AddAuthorization();
        }
    }

    public class JwtSettingsOptions
    {
        public string Issuer { get; set; } = "";
        public string SignKey { get; set; } = "";
        public int ExpireTimeInHour { get; set; } = 0;
    }
}
