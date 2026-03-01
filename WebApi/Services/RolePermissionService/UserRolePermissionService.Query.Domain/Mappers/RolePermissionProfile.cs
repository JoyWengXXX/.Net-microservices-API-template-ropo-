using AutoMapper;
using SystemMain.Entities;
using RolePermissionService.Query.Domain.DTOs;

namespace RolePermissionService.Query.Domain.Mappers
{
    public class RolePermissionProfile : Profile
    {
        public RolePermissionProfile()
        {
            CreateMap<UserRoleBinding, PermissionsReturnDTO>()
                .ForMember(x => x.controllerId, y => y.MapFrom(o => o.ControllerId))
                .ForMember(x => x.createAllowed, y => y.MapFrom(o => o.CreateAllowed))
                .ForMember(x => x.queryAllowed, y => y.MapFrom(o => o.QueryAllowed))
                .ForMember(x => x.updateAllowed, y => y.MapFrom(o => o.UpdateAllowed))
                .ForMember(x => x.deleteAllowed, y => y.MapFrom(o => o.DeleteAllowed));
        }
    }
}

