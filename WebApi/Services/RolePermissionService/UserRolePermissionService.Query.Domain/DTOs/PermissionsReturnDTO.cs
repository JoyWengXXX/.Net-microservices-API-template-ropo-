

namespace RolePermissionService.Query.Domain.DTOs
{
    public class PermissionsReturnDTO
    {
        public string controllerId { get; set; }
        public bool createAllowed { get; set; }
        public bool updateAllowed { get; set; }
        public bool deleteAllowed { get; set; }
        public bool queryAllowed { get; set; }
    }
}

