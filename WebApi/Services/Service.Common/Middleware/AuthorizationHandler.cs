using DataAccess;
using DataAccess.Interfaces;
using SystemMain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Common.Models;
using Service.Common.Models.DTOs;
using System.Linq.Expressions;

namespace Service.Common.Middleware
{
    public class AuthorizationHandler : ControllerBase
    {
        private readonly RequestDelegate _next;

        public AuthorizationHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IRepository<MainDBConnectionManager> logInRecordRepository)
        {
            // 取得Request的Header中的JWT TOKEN
            string Token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(Token))
            {
                Expression<Func<LogInRecord, object>> selected = e => new { e.Token, e.IsEnable };
                Expression<Func<LogInRecord, bool>> whereConditions = e => e.Token == Token;
                LogInRecord? logInRecord = await logInRecordRepository.GetFirstAsync(selected, whereConditions);
                var response = context.Response;
                response.ContentType = "application/json";
                ObjectResult result;
                if (logInRecord != null && logInRecord.IsEnable)
                {
                    await _next(context);
                }
                else
                {
                    result = Unauthorized(new ResponseDTO 
                    {
                        isSuccess = false,
                        message = "Token fail",
                        resultCode = ReturnResultCodeEnums.SystemResultCode.TokenFail.ToString()
                    });
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    await response.WriteAsync(JsonConvert.SerializeObject(result.Value));
                }
            }
            else
            {
                await _next(context);
                return;
            }
        }
    }
}

