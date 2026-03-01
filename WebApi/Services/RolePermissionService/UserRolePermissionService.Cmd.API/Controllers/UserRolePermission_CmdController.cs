using CommonLibrary.Helpers;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using RolePermissionService.Cmd.API.Commands;
using RolePermissionService.Cmd.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Service.Common.Models.DTOs;

namespace RoleService.Cmd.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserRolePermission_CmdController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public UserRolePermission_CmdController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        /// <summary>
        /// 更新角色底下權限
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("UpdatePermission")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdatePermission([FromBody] UpdateRolePermissionDTO input)
        {
            string commandUUID = $"{input.userId}@{input.storeId}";
            var command = new UpdateRolePermissionCommand
            {
                id = ConverterHelper.StringToGuid(commandUUID),
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

