using AutoMapper;
using CQRS.Core.Infrastructure;
using SystemMain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleService.Query.Domain.DTOs;
using RoleService.Query.Domain.Mappers;
using RoleService.Query.Domain.Queries;
using Service.Common.Filters;
using Service.Common.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace RoleService.Query.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserRole_QueryController : ControllerBase
    {
        private readonly IQueryDispatcher _dispatcher;
        private readonly IMapper _mapper;

        public UserRole_QueryController(IQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            var config = new MapperConfiguration(cfg => cfg.AddProfile<RoleProfile>());
            _mapper = config.CreateMapper();
        }

        /// <summary>
        /// 取得系統角色清單
        /// </summary>
        /// <returns></returns>
        [HttpGet("Roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Roles()
        {
            var result = await _dispatcher.SendAsync(new GetRolesQuery());
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = _mapper.Map<List<QueryRolesDTO>>((List<UserRole>)result.executionData)
            });
        }

        /// <summary>
        /// 取得使用者在指定店家下的系統角色
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpGet("UserRoleInStore")]
        [Authorize]
        [ActionRoleFilter]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> UserRoleInStore([FromQuery][Required] Guid storeId)
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _dispatcher.SendAsync(new GetRoleInStoreQuery() { storeId = storeId, userId = userId });
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = result.executionData
            });
        }
    }
}

