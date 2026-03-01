using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemMain.Entities
{
    /// <summary>
    /// 使用者資訊
    /// </summary>
    [Table("UserInfo")]
    public partial class UserInfo
    {
        /// <summary>
        /// 使用者帳號
        /// </summary>
        [Key]
        [Required]
        [StringLength(254)]
        public string UserId { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [Required]
        [StringLength(256)]
        public string Password { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [StringLength(254)]
        public string? Email { get; set; }

        /// <summary>
        /// 手機
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// 頭貼Url
        /// </summary>
        [StringLength(300)]
        public string? Photo { get; set; }

        /// <summary>
        /// 是否驗證
        /// </summary>
        public bool IsVerify { get; set; }

        /// <summary>
        /// 是否為管理者帳號
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// 第三方登入方式
        /// 0 > 一般
        /// 1 > Google
        /// 2 > iOS
        /// </summary>
        public int SSOType { get; set; }

        /// <summary>
        /// 驗證碼
        /// </summary>
        [StringLength(10)]
        public string? VerificationCode { get; set; }

        /// <summary>
        /// 驗證日期
        /// </summary>
        public DateTime? VerificationDate { get; set; }

        /// <summary>
        /// 帳號狀態
        /// 1:停用
        /// 2:啟用中
        /// </summary>
        [Column(TypeName = "smallint")]
        public int UserStatus { get; set; } = 1;

        /// <summary>
        /// 狀態備註
        /// </summary>
        [StringLength(100)]
        public string? StatusMemo { get; set; }

        /// <summary>
        /// 創建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(300)]
        public string? Memo { get; set; }

        /// <summary>
        /// 時區ID
        /// </summary>
        public string TimeZoneID { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public virtual UserRole UserRole { get; set; }
    }
}


