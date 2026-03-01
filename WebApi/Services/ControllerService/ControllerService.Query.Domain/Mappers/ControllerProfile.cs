using AutoMapper;
using SystemMain.Entities;
using ControllerService.Query.Domain.DTOs;

namespace ControllerService.Query.Domain.Mappers
{
    public class ControllerProfile : Profile
    {
        public ControllerProfile()
        {
            CreateMap<Controller, GetControllerDTO>()
                .ForMember(x => x.controllerId, y => y.MapFrom(o => o.ControllerId))
                .ForMember(x => x.controllerName, y => y.MapFrom(o => o.ControllerName));
        }
    }
}

