using CQRS.Core.Infrastructure;
using SignUpService.Query.Domain.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Common.Models.DTOs;
using System.Security.Claims;
using SignUpService.Query.Domain.DTOs;

namespace SignUpService.Query.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SignUp_QueryController : ControllerBase
    {
        private readonly IQueryDispatcher _dispatcher;

        public SignUp_QueryController(IQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// 取得個人資訊
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("UserInfo")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> UserInfo()
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var result = await _dispatcher.SendAsync(new UserInfoQuery() { userId = userId });
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = result.executionData
            });
        }

        /// <summary>
        /// 取得所有使用者資訊
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("AllUser")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> AllUser([FromQuery] QueryAllUserDTO input)
        {
            var result = await _dispatcher.SendAsync(new AllUserQuery() { input = input });
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = result.executionData
            });
        }
    }
}

