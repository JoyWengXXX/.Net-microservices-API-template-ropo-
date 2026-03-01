using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemMain.Entities
{
    /// <summary>
    /// 系統功能
    /// </summary>
    [Table("Controller")]
    public partial class Controller
    {
        /// <summary>
        /// 控制器編號
        /// </summary>
        [Key]
        [Required]
        [StringLength(50)]
        public string ControllerId { get; set; }

        /// <summary>
        /// 控制器名稱
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ControllerName { get; set; }

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


