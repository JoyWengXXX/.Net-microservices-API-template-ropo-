
namespace Service.Common.Models
{
    public class TokenUserInfo
    {
        public string userId { get; set; }
        public string role { get; set; }
        public int roleOrder { get; set; }
        public string photoUrl { get; set; }
        public string timeZoneId { get; set; }
        public bool goodsIsVisibleSwitch { get; set; }
    }
}

