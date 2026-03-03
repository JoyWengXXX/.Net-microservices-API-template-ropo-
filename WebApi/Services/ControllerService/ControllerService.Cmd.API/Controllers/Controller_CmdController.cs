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
        /// 新增功能控制器
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDTO>> Add([FromBody] AddControllerDTO input)
        {
            var command = new AddControllerCommand
            {
                id = ConverterHelper.StringToGuid(input.controllerId),
                input = input,
                createOn = DateTime.UtcNow,
            };
            await _commandDispatcher.SendAsync(command);
            return StatusCode(StatusCodes.Status201Created, new ResponseDTO
            {
                isSuccess = true,
                message = "Success"
            });
        }

        /// <summary>
        /// 更新功能控制器
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("Update")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDTO>> Update([FromBody] UpdateControllerDTO input)
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
        /// 停用功能控制器
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        [HttpDelete("{controllerId}")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDTO>> Disable([FromRoute] string controllerId)
        {
            var command = new DisableControllerCommand
            {
                id = ConverterHelper.StringToGuid(controllerId),
                ControllerId = controllerId,
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

