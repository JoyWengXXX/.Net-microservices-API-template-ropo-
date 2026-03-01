using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemMain.Entities
{
    /// <summary>
    /// 使用者角色綁定
    /// </summary>
    [Table("UserRoleBinding")]
    public partial class UserRoleBinding
    {
        /// <summary>
        /// 使用者編號
        /// </summary>
        [Key]
        [StringLength(254)]
        public string UserId { get; set; }

        /// <summary>
        /// 角色編號
        /// </summary>
        [Key]
        public Guid RoleId { get; set; }

        /// <summary>
        /// 控制器編號
        /// </summary>
        [Key]
        [StringLength(50)]
        public string ControllerId { get; set; }

        /// <summary>
        /// 是否允許新增
        /// </summary>
        [Required]
        public bool CreateAllowed { get; set; }

        /// <summary>
        /// 是否允許查詢
        /// </summary>
        [Required]
        public bool QueryAllowed { get; set; }

        /// <summary>
        /// 是否允許更新
        /// </summary>
        [Required]
        public bool UpdateAllowed { get; set; }

        /// <summary>
        /// 是否允許刪除
        /// </summary>
        [Required]
        public bool DeleteAllowed { get; set; }

        /// <summary>
        /// 創建日期
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; }


        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public virtual UserInfo UserInfo { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public virtual UserRole UserRole { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public virtual Controller Controller { get; set; }
    }
}


