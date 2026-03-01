
namespace LogInService.Cmd.Domain.Models
{
    public class UserWithPermissions
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public int SSOType { get; set; }
        public string TimeZoneID { get; set; }
        public bool GoodsIsVisibleSwitch { get; set; }
        public bool IsAdmin { get; set; }
        public Guid StoreId { get; set; }
        public Guid RoleId { get; set; }
        public int RoleOrder { get; set; }
        public string RoleName { get; set; }
        public byte[] PermissionCheckBit { get; set; }
        public string ControllerId { get; set; }
        public bool CreateAllowed { get; set; }
        public bool UpdateAllowed { get; set; }
        public bool DeleteAllowed { get; set; }
        public bool QueryAllowed { get; set; }
    }
}

