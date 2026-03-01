using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemMain.Entities
{
    /// <summary>
    /// 使用者角色
    /// </summary>
    [Table("UserRole")]
    public partial class UserRole
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Key]
        public Guid RoleId { get; set; }

        /// <summary>
        /// 角色名稱
        /// </summary>
        [Required]
        [StringLength(20)]
        public string RoleName { get; set; }

        /// <summary>
        /// 角色排序
        /// </summary>
        [Required]
        [Column(TypeName = "smallint")]
        public int RoleOrder { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Required]
        public bool IsEnable { get; set; }

        /// <summary>
        /// 創建日期
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}


