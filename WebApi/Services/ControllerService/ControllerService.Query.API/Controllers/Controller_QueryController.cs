using AutoMapper;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControllerService.Query.Domain.DTOs;
using ControllerService.Query.Domain.Queries;
using Service.Common.Models.DTOs;

namespace ControllerService.Query.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/v1/[controller]")]
    public class Controller_QueryController : ControllerBase
    {
        private readonly IQueryDispatcher _pageDispatcher;
        private readonly IMapper _mapper;

        public Controller_QueryController(IQueryDispatcher pageDispatcher, IMapper mapper)
        {
            _pageDispatcher = pageDispatcher;
            _mapper = mapper;
        }

        /// <summary>
        /// 取得控制器功能頁清單
        /// </summary>
        [HttpGet("Controllers")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDTO>> Controllers()
        {
            var result = await _pageDispatcher.SendAsync(new GetControllersQuery());
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = _mapper.Map<List<GetControllerDTO>>((List<SystemMain.Entities.Controller>)result.executionData)
            });
        }

        /// <summary>
        /// 取得控制器功能頁內容
        /// </summary>
        /// <param name="controllerId"></param>
        [HttpGet("Controller/{controllerId}")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDTO>> GetController(string controllerId)
        {
            var result = await _pageDispatcher.SendAsync(new GetControllerByIdQuery { ControllerId = controllerId });
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = _mapper.Map<GetControllerDTO>((SystemMain.Entities.Controller)result.executionData)
            });
        }
    }
}
