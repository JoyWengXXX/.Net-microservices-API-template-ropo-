
namespace Service.Common.Models.DTOs
{
    public class StoreRoleDTO
    {
        public string userId { get; set; }
        public bool isAdmin {  get; set; }
        public int roleOrder {  get; set; }
        public string timeZoneId {  get; set; }
        public bool goodsIsVisibleSwitch {  get; set; }
        public List<PermissionCheckBitInStores> permissionCheckBitsInStores { get; set; } = new List<PermissionCheckBitInStores>();
        public List<UserRoleInStores> userRoleInStores { get; set; } = new List<UserRoleInStores>();
    }

    public class UserRoleInStores
    {
        public Guid storeId { get; set; }
        public Guid roleId { get; set; }
        public int roleOrder { get; set; }
        public string roleName { get; set; }
        public List<RolePermissions> rolePermissions { get; set; }
    }

    public class PermissionCheckBitInStores
    {
        public Guid storeId { get; set; }
        public byte[] permissionCheckBit { get; set; }
    }

    public class RolePermissions
    {
        public string controllerId { get; set; }
        public Dictionary<string, bool> permissions { get; set; }
    }
}

