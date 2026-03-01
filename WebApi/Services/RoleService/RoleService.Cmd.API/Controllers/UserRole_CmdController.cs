using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using RoleService.Cmd.API.Commands;
using RoleService.Cmd.Domain.DTOs;
using Service.Common.Filters;
using Microsoft.AspNetCore.Authorization;
using Service.Common.Models.DTOs;

namespace RoleService.Cmd.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]
    [ActionRoleFilter]
    public class UserRole_CmdController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public UserRole_CmdController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Add([FromBody] AddRoleDTO input)
        {
            var command = new AddRoleCommand
            {
                id = Guid.NewGuid(),
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
        /// 更新角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("Update")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Update([FromBody] UpdateRoleDTO input)
        {
            var command = new UpdateRoleCommand
            {
                id = input.roleId,
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
        /// 停用角色
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpPut("Disable")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Disable([FromBody] Guid roleId)
        {
            var command = new DisableRoleCommand
            {
                id = roleId,
                roleId = roleId,
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

