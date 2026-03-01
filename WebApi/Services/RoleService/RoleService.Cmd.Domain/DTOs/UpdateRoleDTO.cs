using System.ComponentModel.DataAnnotations;

namespace RoleService.Cmd.Domain.DTOs
{
    public class UpdateRoleDTO
    {
        [Required]
        public Guid roleId { get; set; }

        [Required]
        public string roleName { get; set; }

        [Required]
        public int roleOrder { get; set; }
    }
}

