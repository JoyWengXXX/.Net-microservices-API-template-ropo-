using AutoMapper;
using UserRolePermissionService.Cmd.Domain.Events;
using RolePermissionService.Cmd.Domain.DTOs;

namespace RolePermissionService.Cmd.Domain.Mappers
{
    public class RolePermissionProfile : Profile
    {
        public RolePermissionProfile()
        {
            CreateMap<PermissionsDTO, UserRoleBinding>()
                .ForMember(x => x.ControllerId, y => y.MapFrom(o => o.controllerId))
                .ForMember(x => x.QueryAllowed, y => y.MapFrom(o => o.queryAllowed))
                .ForMember(x => x.CreateAllowed, y => y.MapFrom(o => o.createAllowed))
                .ForMember(x => x.UpdateAllowed, y => y.MapFrom(o => o.updateAllowed))
                .ForMember(x => x.DeleteAllowed, y => y.MapFrom(o => o.deleteAllowed));
        }
    }
}

