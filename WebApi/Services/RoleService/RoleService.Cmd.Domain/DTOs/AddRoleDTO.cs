using System.ComponentModel.DataAnnotations;

namespace RoleService.Cmd.Domain.DTOs
{
    public class AddRoleDTO
    {
        [Required]
        public string roleName { get; set; }
        [Required]
        public int roleOrder { get; set; }
    }
}

