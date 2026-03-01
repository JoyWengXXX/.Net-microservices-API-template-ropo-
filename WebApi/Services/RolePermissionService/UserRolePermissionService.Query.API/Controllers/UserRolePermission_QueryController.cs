using AutoMapper;
using CQRS.Core.Infrastructure;
using SystemMain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RolePermissionService.Query.Domain.DTOs;
using RolePermissionService.Query.Domain.Mappers;
using RolePermissionService.Query.Domain.Queries;
using Service.Common.Filters;
using Service.Common.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RolePermissionService.Query.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [ActionRoleFilter]
    public class UserRolePermission_QueryController : ControllerBase
    {
        private readonly IQueryDispatcher _dispatcher;
        private readonly IMapper _mapper;

        public UserRolePermission_QueryController(IQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            var config = new MapperConfiguration(cfg => cfg.AddProfile<RolePermissionProfile>());
            _mapper = config.CreateMapper();
        }

        /// <summary>
        /// 取得角色底下權限
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("Permissions")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Permissions([Required][FromQuery] PermissionsQueryDTO input)
        {
            var result = await _dispatcher.SendAsync(new GetPermissionsQuery() { input = input });
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = _mapper.Map<List<PermissionsReturnDTO>>((List<UserRoleBinding>)result.executionData)
            });
        }
    }
}

