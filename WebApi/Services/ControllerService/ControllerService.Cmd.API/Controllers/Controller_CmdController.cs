using CommonLibrary.Helpers;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Common.Models.DTOs;
using ControllerService.Cmd.Domain.DTOs;
using ControllerService.Cmd.API.Commands;

namespace ControllerService.Cmd.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]
    public class Controller_CmdController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public Controller_CmdController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        /// <summary>
        /// 新增功能頁
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Add([FromBody] AddControllerDTO input)
        {
            var command = new AddControllerCommand
            {
                id = ConverterHelper.StringToGuid(input.controllerId),
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
        /// 更新功能
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("Update")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Update([FromBody] UpdateControllerDTO input)
        {
            var command = new UpdateControllerCommand
            {
                id = ConverterHelper.StringToGuid(input.controllerId),
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
        /// 停用功能頁
        /// </summary>
        /// <param name="ControllerId"></param>
        /// <returns></returns>
        [HttpPut("Disable")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Disable([FromBody] string ControllerId)
        {
            var hashBytes = EncryptDecryptHelper.Instance.CreateHash(ControllerId, EncryptDecryptHelper.HashType.SHA256);
            var command = new DisableControllerCommand
            {
                id = ConverterHelper.StringToGuid(ControllerId),
                ControllerId = ControllerId,
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

