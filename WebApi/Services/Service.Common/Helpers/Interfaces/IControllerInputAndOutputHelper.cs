
namespace Service.Common.Helpers.Interfaces
{
    public interface IControllerInputAndOutputHelper
    {
        /// <summary>
        /// 依照時區ID獲取時間偏移量
        /// </summary>
        public int GetTimeZoneOffset(string timeZoneId);

        /// <summary>
        /// 獲取使用者時區ID
        /// </summary>
        /// <returns>returns>
        public string GetUserTimeZoneID(string IDType = "Linux", int? timeZoneOffset = null);

        /// <summary>
        /// 獲取使用者時區的時間偏移量
        /// </summary>
        /// <returns>returns>
        public int GetUserTimeZoneOffset();

        /// <summary>
        /// 檢查控制器的店鋪編號是否符合使用者身分
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public Task<KeyValuePair<Guid, int>> StoreIdCheck(string userId, Guid storeId);

        /// <summary>
        /// 依照使用者的時區ID轉換成UTC時間
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public DateTime ToUTCWithUserTimeZone(DateTime date);

        /// <summary>
        /// 依照時區ID將UTC時間轉成使用者的時間
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public DateTime ToUserTimeZone(DateTime date);
    }
}

