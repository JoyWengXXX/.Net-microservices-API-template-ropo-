using System.ComponentModel.DataAnnotations;

namespace RoleService.Cmd.Domain.DTOs
{
    public class UpdateRolePermissionDTO
    {
        [Required]
        public int userId { get; set; }
        [Required]
        public Guid storeId { get; set; }

        [Required]
        public List<PermissionsDTO> permissions { get; set; }
    }

    public class PermissionsDTO
    {
        [Required]
        public string controllerId { get; set; }

        [Required]
        public bool createAllowed { get; set; }

        [Required]
        public bool queryAllowed { get; set; }

        [Required]
        public bool updateAllowed { get; set; }

        [Required]
        public bool deleteAllowed { get; set; }
    }
}

