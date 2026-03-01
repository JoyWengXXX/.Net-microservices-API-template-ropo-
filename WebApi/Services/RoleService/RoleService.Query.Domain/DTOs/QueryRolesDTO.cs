using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleService.Query.Domain.DTOs
{
    public class QueryRolesDTO
    {
        public Guid roleId { get; set; }
        public string roleName { get; set; }
        public bool isEnable { get; set; }
    }
}

