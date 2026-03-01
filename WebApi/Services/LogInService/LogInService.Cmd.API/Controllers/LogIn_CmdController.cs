using static Service.Common.Models.UserRoleEnums;
using CommonLibrary.Helpers;
using CQRS.Core.Infrastructure;
using Google.Apis.Auth;
using LogInService.Cmd.Commands;
using LogInService.Cmd.Domain.DTOs;
using LogInService.Cmd.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Common.Filters;
using Service.Common.Models.DTOs;

namespace LogInService.Cmd.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LogIn_CmdController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public LogIn_CmdController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        /// <summary>
        /// 登入帳號
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("LogIn")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> LogIn([FromBody] LogInDTO input)
        {
            input.password = Convert.ToBase64String(EncryptDecryptHelper.Instance.CreateHash(input.password, EncryptDecryptHelper.HashType.SHA256)).Replace("-", "");
            var command = new LogInCommand
            {
                id = ConverterHelper.StringToGuid(input.userId),
                input = input,
                createOn = DateTime.UtcNow,
            };
            var authenticationResult = await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "LogIn Success",
                result = authenticationResult.executionData
            });
        }

        /// <summary>
        /// Google登入
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("GoogleLogIn")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> GoogleLogIn(GoogleLogInDTO input)
        {
            GoogleCredentialPayload credencialPayload = new GoogleCredentialPayload();
            try
            {
                // 解析個人資訊
                var credential = await GoogleJsonWebSignature.ValidateAsync(input.credential);
                credencialPayload.email = credential.Email;
                credencialPayload.name = credential.Name;
                credencialPayload.photo = credential.Picture;
            }
            catch
            {
                return BadRequest(new TResult
                {
                    isSuccess = false,
                    executionData = "Invalid Google token"
                });
            }

            var command = new GoogleLogInCommand
            {
                id = ConverterHelper.StringToGuid(credencialPayload.email),
                input = credencialPayload
            };
            var authenticationResult = await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "LogIn Success",
                result = authenticationResult.executionData
            });
        }

        /// <summary>
        /// 登入管理者帳號
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("AdminLogIn")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> AdminLogIn([FromBody] LogInDTO input)
        {
            input.password = Convert.ToBase64String(EncryptDecryptHelper.Instance.CreateHash(input.password, EncryptDecryptHelper.HashType.SHA256)).Replace("-", "");
            var command = new AdminLogInCommand
            {
                id = ConverterHelper.StringToGuid(input.userId),
                input = input,
                createOn = DateTime.UtcNow,
            };
            var authenticationResult = await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "LogIn Success",
                result = authenticationResult.executionData
            });
        }

        /// <summary>
        /// 登出帳號
        /// </summary>
        /// <returns></returns>
        [HttpPost("LogOut")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> LogOut()
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var command = new LogOutCommand
            {
                id = ConverterHelper.StringToGuid(userId),
                token = token,
                createOn = DateTime.UtcNow,
            };
            await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Logout success"
            });
        }

        /// <summary>
        /// 開啟主動通知
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("EnableNotification")]
        [Authorize]
        [ActionRoleFilter]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> EnableNotification([FromBody] EnableNotificationDTO input)
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
            var command = new EnableNotificationCommand
            {
                id = ConverterHelper.StringToGuid(userId),
                input = input,
                createOn = DateTime.UtcNow,
            };
            await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success"
            });
        }
    }
}

