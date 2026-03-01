using static Service.Common.Models.ReturnResultCodeEnums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Common.Models.DTOs;
using Serilog.Context;

namespace Service.Common.Middleware
{
    /// <summary>
    /// 處理Exception發生時的步驟
    /// 掛在MiddleWare中
    /// </summary>
    public class ErrorHandler : ControllerBase
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandler> _logger;

        public ErrorHandler(RequestDelegate next,
                            ILogger<ErrorHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                var response = context.Response;
                response.ContentType = "application/json";
                ResponseDTO result;
                //如果是自定義的錯誤，則回傳客製化的錯誤訊息
                if (error is AppException)
                {
                    var throwedException = (AppException)error;
                    result = new ResponseDTO
                    {
                        result = null,
                        isSuccess = throwedException.isSuccess,
                        message = throwedException.Message,
                        resultCode = throwedException.code == 0 ? null : throwedException.code.ToString()
                    };
                    response.StatusCode = throwedException.httpStatusCode;
                }
                else
                {
                    result = new ResponseDTO
                    {
                        result = null,
                        isSuccess = false,
                        message = "Internal Server Error ",
                        resultCode = SystemResultCode.ServerInternalError.ToString()
                    };
                    response.StatusCode = StatusCodes.Status500InternalServerError;

                    // 記錄系統異常
                    using (LogContext.PushProperty("RequestPath", context.Request.Path))
                    using (LogContext.PushProperty("RequestMethod", context.Request.Method))
                    using (LogContext.PushProperty("RequestBody", JsonConvert.SerializeObject(result)))
                    {
                        _logger.LogError(error,
                            "系統異常: {ErrorMessage}, StackTrace: {StackTrace}",
                            error.Message,
                            error.StackTrace);
                    }
                }

                await response.WriteAsync(JsonConvert.SerializeObject(result));
            }
        }
    }

    //預設自定義Exception
    public class AppException : Exception
    {
        public SystemResultCode code;
        public int httpStatusCode;
        public bool isSuccess;
        public AppException(string message, SystemResultCode code = 0, bool isSuccess = false, int httpStatusCode = StatusCodes.Status400BadRequest) : base(message)
        {
            this.code = code;
            this.isSuccess = isSuccess;
            this.httpStatusCode = httpStatusCode;
        }
    }
}

