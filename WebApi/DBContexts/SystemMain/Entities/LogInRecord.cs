using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemMain.Entities
{
    /// <summary>
    /// 使用者登入紀錄
    /// </summary>
    [Table("LogInRecord")]
    public partial class LogInRecord
    {
        /// <summary>
        /// 使用者編號
        /// </summary>
        [Key]
        [StringLength(254)]
        public string UserId { get; set; }

        /// <summary>
        /// 登入Token
        /// </summary>
        [Key]
        [Required]
        [StringLength(5000)]
        public string Token { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Required]
        public bool IsEnable { get; set; }
        
        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; }
    }
}


