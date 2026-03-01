using AutoMapper;
using SystemMain.Entities;
using RoleService.Query.Domain.DTOs;

namespace RoleService.Query.Domain.Mappers
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<UserRole, QueryRolesDTO>()
                .ForMember(x => x.roleId, y => y.MapFrom(o => o.RoleId))
                .ForMember(x => x.roleName, y => y.MapFrom(o => o.RoleName))
                .ForMember(x => x.isEnable, y => y.MapFrom(o => o.IsEnable));
        }
    }
}

