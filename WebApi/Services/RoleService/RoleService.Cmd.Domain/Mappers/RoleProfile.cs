using AutoMapper;
using SystemMain.Entities;
using RoleService.Cmd.Domain.DTOs;

namespace RoleService.Cmd.Domain.Mappers
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<UserRoleBinding, PermissionsDTO>()
                .ForMember(x => x.controllerId, y => y.MapFrom(o => o.ControllerId))
                .ForMember(x => x.queryAllowed, y => y.MapFrom(o => o.QueryAllowed))
                .ForMember(x => x.createAllowed, y => y.MapFrom(o => o.CreateAllowed))
                .ForMember(x => x.updateAllowed, y => y.MapFrom(o => o.UpdateAllowed))
                .ForMember(x => x.deleteAllowed, y => y.MapFrom(o => o.DeleteAllowed))
                .ForMember(x => DateTime.UtcNow, y => y.MapFrom(o => o.CreateDate));
        }
    }
}

