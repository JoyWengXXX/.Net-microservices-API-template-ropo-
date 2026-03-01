using CommonLibrary.Helpers;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Common.Models.DTOs;
using SignUpService.Cmd.API.Commands;
using SignUpService.Cmd.Domain.DTOs;
using SignUpService.Cmd.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SignUpService.Cmd.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SignUp_CmdController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly string AESKey;

        public SignUp_CmdController(ICommandDispatcher commandDispatcher, IConfiguration configuration)
        {
            _commandDispatcher = commandDispatcher;
            DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var sysConfig = ConfigurationHelper.LoadConfiguration(
                directoryInfo.FullName,
                "CommonSettings"
            );
            AESKey = sysConfig.GetValue<string>("SystemConfig:AESEncryptKey");
        }

        /// <summary>
        /// 註冊帳號
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("SignUp")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> SignUp([FromBody] SignUpDTO input)
        {
            var command = new SignInCommand
            {
                id = ConverterHelper.StringToGuid(input.userId),
                input = input,
                createOn = DateTime.UtcNow,
            };
            await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "SignUp Success"
            });
        }

        /// <summary>
        /// 驗證帳號
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("Validate")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Validate([FromBody] string input)
        {
            var decodedParamsStr = EncryptDecryptHelper.Instance.AESDecrypt(input, AESKey);
            var decodedParams = JsonSerializer.Deserialize<ValidateParameters>(decodedParamsStr);
            var command = new ValidateCommand
            {
                id = ConverterHelper.StringToGuid(decodedParams.userId),
                input = decodedParams,
                createOn = DateTime.UtcNow,
            };
            await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Validate Success"
            });
        }

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("ForgetPassword")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> ForgetPassword([FromBody] ForgetPasswordDTO input)
        {
            var command = new ForgetPasswordCommand
            {
                id = ConverterHelper.StringToGuid(input.userId),
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

        /// <summary>
        /// 變更密碼
        /// </summary>
        /// <param name="input">newPassword</param>
        /// <returns></returns>
        [HttpPut("Password")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Password([FromBody] [Required] string input)
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
            var command = new PasswordChangeCommand
            {
                id = ConverterHelper.StringToGuid(userId),
                newPW = input,
                createOn = DateTime.UtcNow,
            };
            await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success"
            });
        }

        /// <summary>
        /// 變更使用者資訊
        /// </summary>
        /// <param name="input">userName</param>
        /// <returns></returns>
        [HttpPut("UserInfo")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> UserInfo([FromBody] UpdateUserInfosDTO input)
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
            var command = new UserInfoChangeCommand
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

        /// <summary>
        /// 停用帳號
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut("AccountDisable")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> AccountDisable([FromBody][Required] string userId)
        {
            var command = new AccountDisableCommand
            {
                id = ConverterHelper.StringToGuid(userId),
                userId = userId,
                createOn = DateTime.UtcNow,
            };
            await _commandDispatcher.SendAsync(command);
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success"
            });
        }

        /// <summary>
        /// 刪除帳號
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("AccountDelete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> AccountDelete([FromBody][Required] string userId)
        {
            var command = new AccountDeleteCommand
            {
                id = ConverterHelper.StringToGuid(userId),
                userId = userId,
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

