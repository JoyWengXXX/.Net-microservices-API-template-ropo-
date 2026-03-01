using AutoMapper;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControllerService.Query.Domain.DTOs;
using ControllerService.Query.Domain.Mappers;
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

        public Controller_QueryController(IQueryDispatcher pageDispatcher)
        {
            _pageDispatcher = pageDispatcher;
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ControllerProfile>());
            _mapper = config.CreateMapper();
        }

        /// <summary>
        /// 取得系統功能頁清單
        /// </summary>
        /// <returns></returns>
        [HttpGet("Controllers")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Controllers()
        {
            var result = await _pageDispatcher.SendAsync(new GetControllersQuery());
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = _mapper.Map<List<GetControllerDTO>> (result)
            });
        }

        /// <summary>
        /// 取得系統功能頁內容
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        [HttpGet("Controller")]
        [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> Controller(string controllerId)
        {
            var result = await _pageDispatcher.SendAsync(new GetControllersQuery());
            List<SystemMain.Entities.Controller> pages = (List<SystemMain.Entities.Controller>)result.executionData;
            return Ok(new ResponseDTO
            {
                isSuccess = true,
                message = "Success",
                result = _mapper.Map<GetControllerDTO>(pages.First(x => x.ControllerId == controllerId))
            });
        }
    }
}


