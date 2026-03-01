using System.ComponentModel.DataAnnotations;

namespace RolePermissionService.Query.Domain.DTOs
{
    public class PermissionsQueryDTO
    {
        [Required]
        public string userId { get; set; }
    }
}

