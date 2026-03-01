using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Service.Common.Helpers.Interfaces;
using Service.Common.Middleware;
using Service.Common.Models;
using Service.Common.Models.DTOs;
using StackExchange.Redis;
using System.Runtime.InteropServices;

namespace Service.Common.Helpers
{
    /// <summary>
    /// 通用控制器輸入輸出幫助類別
    /// </summary>
    public class ControllerInputAndOutputHelper : IControllerInputAndOutputHelper
    {
        #region 時區映射常數
        // 時區偏移量對應的 Linux 格式時區 ID
        private static readonly Dictionary<int, string> OffsetToTimeZoneLinux = new Dictionary<int, string>
        {
            { -12, "Etc/GMT+12" },
            { -11, "Pacific/Samoa" },
            { -10, "Pacific/Honolulu" },
            { -9, "America/Anchorage" },
            { -8, "America/Los_Angeles" },
            { -7, "America/Denver" },
            { -6, "America/Chicago" },
            { -5, "America/New_York" },
            { -4, "America/Halifax" },
            { -3, "America/Buenos_Aires" },
            { -2, "America/Noronha" },
            { -1, "Atlantic/Azores" },
            { 0, "Etc/UTC" },
            { 1, "Europe/Berlin" },
            { 2, "Europe/Athens" },
            { 3, "Asia/Baghdad" },
            { 4, "Asia/Dubai" },
            { 5, "Asia/Kolkata" },
            { 6, "Asia/Dhaka" },
            { 7, "Asia/Bangkok" },
            { 8, "Asia/Taipei" },
            { 9, "Asia/Tokyo" },
            { 10, "Australia/Sydney" },
            { 11, "Pacific/Guadalcanal" },
            { 12, "Pacific/Auckland" },
            { 13, "Pacific/Tongatapu" },
            { 14, "Pacific/Kiritimati" }
        };
        
        // 時區偏移量對應的 Windows 格式時區 ID
        private static readonly Dictionary<int, string> OffsetToTimeZoneWindows = new Dictionary<int, string>
        {
            { -12, "Dateline Standard Time" },
            { -11, "Samoa Standard Time" },
            { -10, "Hawaiian Standard Time" },
            { -9, "Alaskan Standard Time" },
            { -8, "Pacific Standard Time" },
            { -7, "Mountain Standard Time" },
            { -6, "Central Standard Time" },
            { -5, "Eastern Standard Time" },
            { -4, "Atlantic Standard Time" },
            { -3, "Argentina Standard Time" },
            { -2, "UTC-02" },
            { -1, "Azores Standard Time" },
            { 0, "UTC" },
            { 1, "W. Europe Standard Time" },
            { 2, "GTB Standard Time" },
            { 3, "Arabic Standard Time" },
            { 4, "Arabian Standard Time" },
            { 5, "India Standard Time" },
            { 6, "Bangladesh Standard Time" },
            { 7, "SE Asia Standard Time" },
            { 8, "Taipei Standard Time" },
            { 9, "Tokyo Standard Time" },
            { 10, "AUS Eastern Standard Time" },
            { 11, "Central Pacific Standard Time" },
            { 12, "New Zealand Standard Time" },
            { 13, "Tonga Standard Time" },
            { 14, "Line Islands Standard Time" }
        };
        
        // Linux 格式到 Windows 格式的時區 ID 映射
        private static readonly Dictionary<string, string> LinuxToWindowsMap = new Dictionary<string, string>
        {
            // 東亞
            { "Asia/Taipei", "Taipei Standard Time" },
            { "Asia/Tokyo", "Tokyo Standard Time" },
            { "Asia/Shanghai", "China Standard Time" },
            { "Asia/Hong_Kong", "China Standard Time" },
            { "Asia/Macau", "China Standard Time" },
            { "Asia/Singapore", "Singapore Standard Time" },
            { "Asia/Bangkok", "SE Asia Standard Time" },
            { "Asia/Kuala_Lumpur", "Singapore Standard Time" },
            { "Asia/Jakarta", "SE Asia Standard Time" },
            { "Asia/Manila", "Singapore Standard Time" },
            { "Asia/Seoul", "Korea Standard Time" },
            { "Asia/Pyongyang", "North Korea Standard Time" },
            
            // 南亞和西亞
            { "Asia/Kolkata", "India Standard Time" },
            { "Asia/Colombo", "Sri Lanka Standard Time" },
            { "Asia/Kathmandu", "Nepal Standard Time" },
            { "Asia/Dhaka", "Bangladesh Standard Time" },
            { "Asia/Karachi", "Pakistan Standard Time" },
            { "Asia/Kabul", "Afghanistan Standard Time" },
            { "Asia/Dubai", "Arabian Standard Time" },
            { "Asia/Muscat", "Arabian Standard Time" },
            { "Asia/Riyadh", "Arab Standard Time" },
            { "Asia/Baghdad", "Arabic Standard Time" },
            { "Asia/Tehran", "Iran Standard Time" },
            { "Asia/Jerusalem", "Israel Standard Time" },
            { "Asia/Amman", "Jordan Standard Time" },
            { "Asia/Beirut", "Middle East Standard Time" },
            
            // 北美
            { "America/New_York", "Eastern Standard Time" },
            { "America/Chicago", "Central Standard Time" },
            { "America/Denver", "Mountain Standard Time" },
            { "America/Los_Angeles", "Pacific Standard Time" },
            { "America/Anchorage", "Alaskan Standard Time" },
            { "America/Phoenix", "US Mountain Standard Time" },
            { "America/Indianapolis", "US Eastern Standard Time" },
            { "America/Halifax", "Atlantic Standard Time" },
            { "America/St_Johns", "Newfoundland Standard Time" },
            
            // 加拿大
            { "America/Toronto", "Eastern Standard Time" },
            { "America/Vancouver", "Pacific Standard Time" },
            { "America/Edmonton", "Mountain Standard Time" },
            { "America/Winnipeg", "Central Standard Time" },
            
            // 墨西哥和中美洲
            { "America/Mexico_City", "Central Standard Time (Mexico)" },
            { "America/Chihuahua", "Mountain Standard Time (Mexico)" },
            { "America/Monterrey", "Central Standard Time (Mexico)" },
            { "America/Guatemala", "Central America Standard Time" },
            { "America/Costa_Rica", "Central America Standard Time" },
            { "America/Panama", "Central America Standard Time" },
            
            // 南美
            { "America/Bogota", "SA Pacific Standard Time" },
            { "America/Lima", "SA Pacific Standard Time" },
            { "America/Caracas", "Venezuela Standard Time" },
            { "America/Santiago", "Pacific SA Standard Time" },
            { "America/Buenos_Aires", "Argentina Standard Time" },
            { "America/Sao_Paulo", "E. South America Standard Time" },
            { "America/Noronha", "UTC-02" },
            
            // 歐洲
            { "Europe/London", "GMT Standard Time" },
            { "Europe/Dublin", "GMT Standard Time" },
            { "Europe/Lisbon", "GMT Standard Time" },
            { "Europe/Paris", "Romance Standard Time" },
            { "Europe/Brussels", "Romance Standard Time" },
            { "Europe/Amsterdam", "W. Europe Standard Time" },
            { "Europe/Berlin", "W. Europe Standard Time" },
            { "Europe/Rome", "W. Europe Standard Time" },
            { "Europe/Madrid", "Romance Standard Time" },
            { "Europe/Zurich", "W. Europe Standard Time" },
            { "Europe/Copenhagen", "Romance Standard Time" },
            { "Europe/Stockholm", "W. Europe Standard Time" },
            { "Europe/Vienna", "W. Europe Standard Time" },
            { "Europe/Athens", "GTB Standard Time" },
            { "Europe/Bucharest", "GTB Standard Time" },
            { "Europe/Helsinki", "FLE Standard Time" },
            { "Europe/Kiev", "FLE Standard Time" },
            { "Europe/Istanbul", "Turkey Standard Time" },
            { "Europe/Moscow", "Russian Standard Time" },
            { "Europe/Warsaw", "Central European Standard Time" },
            { "Europe/Prague", "Central European Standard Time" },
            { "Europe/Budapest", "Central European Standard Time" },
            
            // 澳洲和紐西蘭
            { "Australia/Sydney", "AUS Eastern Standard Time" },
            { "Australia/Melbourne", "AUS Eastern Standard Time" },
            { "Australia/Brisbane", "E. Australia Standard Time" },
            { "Australia/Adelaide", "Cen. Australia Standard Time" },
            { "Australia/Darwin", "AUS Central Standard Time" },
            { "Australia/Perth", "W. Australia Standard Time" },
            { "Pacific/Auckland", "New Zealand Standard Time" },
            { "Pacific/Fiji", "Fiji Standard Time" },
            
            // 太平洋區域
            { "Pacific/Honolulu", "Hawaiian Standard Time" },
            { "Pacific/Guam", "West Pacific Standard Time" },
            { "Pacific/Samoa", "Samoa Standard Time" },
            { "Pacific/Guadalcanal", "Central Pacific Standard Time" },
            { "Pacific/Noumea", "Central Pacific Standard Time" },
            { "Pacific/Port_Moresby", "West Pacific Standard Time" },
            
            // 非洲
            { "Africa/Cairo", "Egypt Standard Time" },
            { "Africa/Johannesburg", "South Africa Standard Time" },
            { "Africa/Lagos", "W. Central Africa Standard Time" },
            { "Africa/Nairobi", "E. Africa Standard Time" },
            { "Africa/Casablanca", "Morocco Standard Time" },
            { "Africa/Algiers", "W. Central Africa Standard Time" },
            { "Africa/Tripoli", "Libya Standard Time" },
            { "Africa/Tunis", "W. Central Africa Standard Time" },
            
            // UTC
            { "Etc/UTC", "UTC" },
            { "Etc/GMT", "UTC" }
        };
        
        // Windows 格式到 Linux 格式的時區 ID 映射
        private static readonly Dictionary<string, string> WindowsToLinuxMap = new Dictionary<string, string>
        {
            // 東亞
            { "Taipei Standard Time", "Asia/Taipei" },
            { "Tokyo Standard Time", "Asia/Tokyo" },
            { "China Standard Time", "Asia/Shanghai" },
            { "Singapore Standard Time", "Asia/Singapore" },
            { "SE Asia Standard Time", "Asia/Bangkok" },
            { "Korea Standard Time", "Asia/Seoul" },
            { "North Korea Standard Time", "Asia/Pyongyang" },
            
            // 南亞和西亞
            { "India Standard Time", "Asia/Kolkata" },
            { "Sri Lanka Standard Time", "Asia/Colombo" },
            { "Nepal Standard Time", "Asia/Kathmandu" },
            { "Bangladesh Standard Time", "Asia/Dhaka" },
            { "Pakistan Standard Time", "Asia/Karachi" },
            { "Afghanistan Standard Time", "Asia/Kabul" },
            { "Arabian Standard Time", "Asia/Dubai" },
            { "Arab Standard Time", "Asia/Riyadh" },
            { "Arabic Standard Time", "Asia/Baghdad" },
            { "Iran Standard Time", "Asia/Tehran" },
            { "Israel Standard Time", "Asia/Jerusalem" },
            { "Jordan Standard Time", "Asia/Amman" },
            { "Middle East Standard Time", "Asia/Beirut" },
            
            // 北美
            { "Eastern Standard Time", "America/New_York" },
            { "Central Standard Time", "America/Chicago" },
            { "Mountain Standard Time", "America/Denver" },
            { "Pacific Standard Time", "America/Los_Angeles" },
            { "Alaskan Standard Time", "America/Anchorage" },
            { "US Mountain Standard Time", "America/Phoenix" },
            { "US Eastern Standard Time", "America/Indianapolis" },
            { "Atlantic Standard Time", "America/Halifax" },
            { "Newfoundland Standard Time", "America/St_Johns" },
            
            // 墨西哥和中美洲
            { "Central Standard Time (Mexico)", "America/Mexico_City" },
            { "Mountain Standard Time (Mexico)", "America/Chihuahua" },
            { "Central America Standard Time", "America/Guatemala" },
            
            // 南美
            { "SA Pacific Standard Time", "America/Bogota" },
            { "Venezuela Standard Time", "America/Caracas" },
            { "Pacific SA Standard Time", "America/Santiago" },
            { "Argentina Standard Time", "America/Buenos_Aires" },
            { "E. South America Standard Time", "America/Sao_Paulo" },
            { "UTC-02", "America/Noronha" },
            
            // 歐洲
            { "GMT Standard Time", "Europe/London" },
            { "Romance Standard Time", "Europe/Paris" },
            { "W. Europe Standard Time", "Europe/Berlin" },
            { "GTB Standard Time", "Europe/Bucharest" },
            { "FLE Standard Time", "Europe/Helsinki" },
            { "Turkey Standard Time", "Europe/Istanbul" },
            { "Russian Standard Time", "Europe/Moscow" },
            { "Central European Standard Time", "Europe/Warsaw" },
            
            // 澳洲和紐西蘭
            { "AUS Eastern Standard Time", "Australia/Sydney" },
            { "E. Australia Standard Time", "Australia/Brisbane" },
            { "Cen. Australia Standard Time", "Australia/Adelaide" },
            { "AUS Central Standard Time", "Australia/Darwin" },
            { "W. Australia Standard Time", "Australia/Perth" },
            { "New Zealand Standard Time", "Pacific/Auckland" },
            { "Fiji Standard Time", "Pacific/Fiji" },
            
            // 太平洋區域
            { "Hawaiian Standard Time", "Pacific/Honolulu" },
            { "West Pacific Standard Time", "Pacific/Guam" },
            { "Samoa Standard Time", "Pacific/Samoa" },
            { "Central Pacific Standard Time", "Pacific/Guadalcanal" },
            
            // 非洲
            { "Egypt Standard Time", "Africa/Cairo" },
            { "South Africa Standard Time", "Africa/Johannesburg" },
            { "W. Central Africa Standard Time", "Africa/Lagos" },
            { "E. Africa Standard Time", "Africa/Nairobi" },
            { "Morocco Standard Time", "Africa/Casablanca" },
            { "Libya Standard Time", "Africa/Tripoli" },
            
            // UTC
            { "UTC", "Etc/UTC" }
        };
        #endregion
        
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 通用控制器輸入輸出幫助類別的建構函式
        /// </summary>
        /// <param name="httpContextAccessor">HTTP 上下文訪問器</param>
        /// <param name="redisConnection">Redis 連接複用器</param>
        public ControllerInputAndOutputHelper(IHttpContextAccessor httpContextAccessor,
                                                IConnectionMultiplexer redisConnection)
        {
            _httpContextAccessor = httpContextAccessor;
            _redisConnection = redisConnection;
        }
        
        /// <summary>
        /// 獲取指定時區ID的時間偏移量
        /// </summary>
        /// <param name="timeZoneId">時區ID</param>
        /// <returns>時區的小時偏移量</returns>
        public int GetTimeZoneOffset(string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                throw new AppException("TimeZone ID cannot be null or empty!");
            }

            // 將輸入的時區ID映射到系統支援的格式
            string mappedTimeZoneId = MapTimeZoneId(timeZoneId);

            try
            {
                // 找到對應的時區
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(mappedTimeZoneId);

                // 獲取當前日期時間
                DateTime now = DateTime.UtcNow;

                // 計算時區偏移量（以小時為單位）
                TimeSpan offset = timeZone.GetUtcOffset(now);

                // 返回偏移的小時數
                return (int)offset.TotalHours;
            }
            catch (TimeZoneNotFoundException)
            {
                throw new AppException($"Time zone '{timeZoneId}' was not found on the system.");
            }
            catch (InvalidTimeZoneException)
            {
                throw new AppException($"Time zone '{timeZoneId}' contains invalid data.");
            }
            catch (Exception ex)
            {
                throw new AppException($"Error determining time zone offset: {ex.Message}");
            }
        }

        /// <summary>
        /// 獲取使用者時區ID
        /// </summary>
        /// <param name="IDType">指定要返回的時區ID類型 ("Windows" 或 "Linux")，為 null 時自動根據當前系統決定</param>
        /// <param name="timeZoneOffset">時區偏移量，默認為 8 小時</param>
        /// <returns>使用者的時區ID</returns>
        public string GetUserTimeZoneID(string IDType = "Linux", int? timeZoneOffset = null)
        {
            string timeZoneId = "";
            if (timeZoneOffset == null)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    throw new AppException("Cannot access HTTP context!");
                }
                // 從 JWT Token Claims 中取得 timeZoneId
                var timeZoneIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "timeZoneId");
                // 依照輸入參數IDType來呼叫MapTimeZoneId方法
                timeZoneId = MapTimeZoneId(timeZoneIdClaim?.Value, IDType);
            }
            else
            {
                // 參照 MapTimeZoneId 支援的時區 ID 與其偏移量
                var offsetToTimeZoneLinux = new Dictionary<int, string>
                    {
                        { -12, "Etc/GMT+12" },
                        { -11, "Pacific/Samoa" },
                        { -10, "Pacific/Honolulu" },
                        { -9, "America/Anchorage" },
                        { -8, "America/Los_Angeles" },
                        { -7, "America/Denver" },
                        { -6, "America/Chicago" },
                        { -5, "America/New_York" },
                        { -4, "America/Halifax" },
                        { -3, "America/Buenos_Aires" },
                        { -2, "America/Noronha" },
                        { -1, "Atlantic/Azores" },
                        { 0, "Etc/UTC" },
                        { 1, "Europe/Berlin" },
                        { 2, "Europe/Athens" },
                        { 3, "Asia/Baghdad" },
                        { 4, "Asia/Dubai" },
                        { 5, "Asia/Kolkata" },
                        { 6, "Asia/Dhaka" },
                        { 7, "Asia/Bangkok" },
                        { 8, "Asia/Taipei" },
                        { 9, "Asia/Tokyo" },
                        { 10, "Australia/Sydney" },
                        { 11, "Pacific/Guadalcanal" },
                        { 12, "Pacific/Auckland" },
                        { 13, "Pacific/Tongatapu" },
                        { 14, "Pacific/Kiritimati" }
                    };
                var offset = timeZoneOffset.Value;
                timeZoneId = offsetToTimeZoneLinux.OrderBy(x => Math.Abs(x.Key - offset)).FirstOrDefault().Value;
            }


            return timeZoneId;
        }

        /// <summary>
        /// 依照時區ID將UTC時間轉成使用者的時間
        /// </summary>
        /// <param name="date">UTC 日期時間</param>
        /// <returns>使用者時區的日期時間</returns>
        public DateTime ToUserTimeZone(DateTime date)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new AppException("Cannot access HTTP context!");
            }

            // 從 JWT Token Claims 中取得 timeZoneId
            var timeZoneIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "timeZoneId");
            // 直接送入 timeZoneIdClaim.Value 到 MapTimeZoneId 方法，如果為空值，則 MapTimeZoneId 會依照作業系統選擇預設值
            string mappedTimeZoneId = MapTimeZoneId(timeZoneIdClaim?.Value);

            // 找到對應的時區
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(mappedTimeZoneId);

            // 確保日期時間的 Kind 屬性為 UTC
            DateTime utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            // 從UTC轉換成使用者的時區時間
            return TimeZoneInfo.ConvertTimeFromUtc(utcDate, userTimeZone);
        }

        /// <summary>
        /// 依照使用者的時區ID轉換成UTC時間
        /// </summary>
        /// <param name="date">使用者時區的日期時間</param>
        /// <returns>UTC 日期時間</returns>
        public DateTime ToUTCWithUserTimeZone(DateTime date)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new AppException("Cannot access HTTP context!");
            }

            // 從 JWT Token Claims 中取得 timeZoneId
            var timeZoneIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "timeZoneId");
            // 直接送入 timeZoneIdClaim.Value 到 MapTimeZoneId 方法，如果為空值，則 MapTimeZoneId 會依照作業系統選擇預設值
            string mappedTimeZoneId = MapTimeZoneId(timeZoneIdClaim?.Value);

            // 找到對應的時區
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(mappedTimeZoneId);

            // 確保日期時間的 Kind 屬性為 Unspecified，以防止時區轉換錯誤
            DateTime dateWithUnspecifiedKind = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);

            // 將使用者時區的時間轉換成UTC時間
            return TimeZoneInfo.ConvertTimeToUtc(dateWithUnspecifiedKind, userTimeZone);
        }

        /// <summary>
        /// 檢查控制器的店鋪編號是否符合使用者身分
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <param name="storeId">店鋪ID</param>
        /// <returns>角色ID和角色順序的鍵值對</returns>
        public async Task<KeyValuePair<Guid, int>> StoreIdCheck(string userId, Guid storeId)
        {
            var redisDB = _redisConnection.GetDatabase();
            var rolesInStore = await redisDB.StringGetAsync($"User:{userId}:permissions");
            var ttl = await redisDB.KeyTimeToLiveAsync($"User:{userId}:permissions");
            var permissionJSON = JsonConvert.DeserializeObject<List<UserRoleInStores>>(rolesInStore.ToString());
            if (permissionJSON == null) { throw new AppException("Please login again!", code: ReturnResultCodeEnums.SystemResultCode.PleaseLoginAgain, isSuccess: true, httpStatusCode: StatusCodes.Status200OK); }
            var userPermission = permissionJSON.FirstOrDefault(x => x.storeId == storeId);
            if (userPermission == null) { throw new AppException("Invalid storeId!"); }

            return new KeyValuePair<Guid, int>(userPermission.roleId, userPermission.roleOrder);
        }

        /// <summary>
        /// 映射時區 ID 以適應不同的作業系統 (Windows/Linux)
        /// </summary>
        /// <param name="timeZoneId">原始時區 ID，可為 null</param>
        /// <param name="IDType">指定要返回的時區ID類型 ("Windows" 或 "Linux")，為 null 時自動根據當前系統決定</param>
        /// <returns>映射後的時區 ID</returns>
        private string MapTimeZoneId(string? timeZoneId, string? IDType = null)
        {
            // 如果沒有指定時區ID，默認使用台北
            if (string.IsNullOrEmpty(timeZoneId))
            {
                return (IDType == "Windows" || (IDType == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))) 
                    ? "Taipei Standard Time" 
                    : "Asia/Taipei";
            }

            // Windows 和 Linux 時區 ID 映射
            Dictionary<string, string> linuxToWindowsMap = new Dictionary<string, string>
            {
                // 東亞
                { "Asia/Taipei", "Taipei Standard Time" },
                { "Asia/Tokyo", "Tokyo Standard Time" },
                { "Asia/Shanghai", "China Standard Time" },
                { "Asia/Hong_Kong", "China Standard Time" },
                { "Asia/Macau", "China Standard Time" },
                { "Asia/Singapore", "Singapore Standard Time" },
                { "Asia/Bangkok", "SE Asia Standard Time" },
                { "Asia/Kuala_Lumpur", "Singapore Standard Time" },
                { "Asia/Jakarta", "SE Asia Standard Time" },
                { "Asia/Manila", "Singapore Standard Time" },
                { "Asia/Seoul", "Korea Standard Time" },
                { "Asia/Pyongyang", "North Korea Standard Time" },
                
                // 南亞和西亞
                { "Asia/Kolkata", "India Standard Time" },
                { "Asia/Colombo", "Sri Lanka Standard Time" },
                { "Asia/Kathmandu", "Nepal Standard Time" },
                { "Asia/Dhaka", "Bangladesh Standard Time" },
                { "Asia/Karachi", "Pakistan Standard Time" },
                { "Asia/Kabul", "Afghanistan Standard Time" },
                { "Asia/Dubai", "Arabian Standard Time" },
                { "Asia/Muscat", "Arabian Standard Time" },
                { "Asia/Riyadh", "Arab Standard Time" },
                { "Asia/Baghdad", "Arabic Standard Time" },
                { "Asia/Tehran", "Iran Standard Time" },
                { "Asia/Jerusalem", "Israel Standard Time" },
                { "Asia/Amman", "Jordan Standard Time" },
                { "Asia/Beirut", "Middle East Standard Time" },
                
                // 北美
                { "America/New_York", "Eastern Standard Time" },
                { "America/Chicago", "Central Standard Time" },
                { "America/Denver", "Mountain Standard Time" },
                { "America/Los_Angeles", "Pacific Standard Time" },
                { "America/Anchorage", "Alaskan Standard Time" },
                { "America/Phoenix", "US Mountain Standard Time" },
                { "America/Indianapolis", "US Eastern Standard Time" },
                { "America/Halifax", "Atlantic Standard Time" },
                { "America/St_Johns", "Newfoundland Standard Time" },
                
                // 加拿大
                { "America/Toronto", "Eastern Standard Time" },
                { "America/Vancouver", "Pacific Standard Time" },
                { "America/Edmonton", "Mountain Standard Time" },
                { "America/Winnipeg", "Central Standard Time" },
                
                // 墨西哥和中美洲
                { "America/Mexico_City", "Central Standard Time (Mexico)" },
                { "America/Chihuahua", "Mountain Standard Time (Mexico)" },
                { "America/Monterrey", "Central Standard Time (Mexico)" },
                { "America/Guatemala", "Central America Standard Time" },
                { "America/Costa_Rica", "Central America Standard Time" },
                { "America/Panama", "Central America Standard Time" },
                
                // 南美
                { "America/Bogota", "SA Pacific Standard Time" },
                { "America/Lima", "SA Pacific Standard Time" },
                { "America/Caracas", "Venezuela Standard Time" },
                { "America/Santiago", "Pacific SA Standard Time" },
                { "America/Buenos_Aires", "Argentina Standard Time" },
                { "America/Sao_Paulo", "E. South America Standard Time" },
                { "America/Noronha", "UTC-02" },
                
                // 歐洲
                { "Europe/London", "GMT Standard Time" },
                { "Europe/Dublin", "GMT Standard Time" },
                { "Europe/Lisbon", "GMT Standard Time" },
                { "Europe/Paris", "Romance Standard Time" },
                { "Europe/Brussels", "Romance Standard Time" },
                { "Europe/Amsterdam", "W. Europe Standard Time" },
                { "Europe/Berlin", "W. Europe Standard Time" },
                { "Europe/Rome", "W. Europe Standard Time" },
                { "Europe/Madrid", "Romance Standard Time" },
                { "Europe/Zurich", "W. Europe Standard Time" },
                { "Europe/Copenhagen", "Romance Standard Time" },
                { "Europe/Stockholm", "W. Europe Standard Time" },
                { "Europe/Vienna", "W. Europe Standard Time" },
                { "Europe/Athens", "GTB Standard Time" },
                { "Europe/Bucharest", "GTB Standard Time" },
                { "Europe/Helsinki", "FLE Standard Time" },
                { "Europe/Kiev", "FLE Standard Time" },
                { "Europe/Istanbul", "Turkey Standard Time" },
                { "Europe/Moscow", "Russian Standard Time" },
                { "Europe/Warsaw", "Central European Standard Time" },
                { "Europe/Prague", "Central European Standard Time" },
                { "Europe/Budapest", "Central European Standard Time" },
                
                // 澳洲和紐西蘭
                { "Australia/Sydney", "AUS Eastern Standard Time" },
                { "Australia/Melbourne", "AUS Eastern Standard Time" },
                { "Australia/Brisbane", "E. Australia Standard Time" },
                { "Australia/Adelaide", "Cen. Australia Standard Time" },
                { "Australia/Darwin", "AUS Central Standard Time" },
                { "Australia/Perth", "W. Australia Standard Time" },
                { "Pacific/Auckland", "New Zealand Standard Time" },
                { "Pacific/Fiji", "Fiji Standard Time" },
                
                // 太平洋區域
                { "Pacific/Honolulu", "Hawaiian Standard Time" },
                { "Pacific/Guam", "West Pacific Standard Time" },
                { "Pacific/Samoa", "Samoa Standard Time" },
                { "Pacific/Guadalcanal", "Central Pacific Standard Time" },
                { "Pacific/Noumea", "Central Pacific Standard Time" },
                { "Pacific/Port_Moresby", "West Pacific Standard Time" },
                
                // 非洲
                { "Africa/Cairo", "Egypt Standard Time" },
                { "Africa/Johannesburg", "South Africa Standard Time" },
                { "Africa/Lagos", "W. Central Africa Standard Time" },
                { "Africa/Nairobi", "E. Africa Standard Time" },
                { "Africa/Casablanca", "Morocco Standard Time" },
                { "Africa/Algiers", "W. Central Africa Standard Time" },
                { "Africa/Tripoli", "Libya Standard Time" },
                { "Africa/Tunis", "W. Central Africa Standard Time" },
                
                // UTC
                { "Etc/UTC", "UTC" },
                { "Etc/GMT", "UTC" }
            };

            Dictionary<string, string> windowsToLinuxMap = new Dictionary<string, string>
            {
                // 東亞
                { "Taipei Standard Time", "Asia/Taipei" },
                { "Tokyo Standard Time", "Asia/Tokyo" },
                { "China Standard Time", "Asia/Shanghai" },
                { "Singapore Standard Time", "Asia/Singapore" },
                { "SE Asia Standard Time", "Asia/Bangkok" },
                { "Korea Standard Time", "Asia/Seoul" },
                { "North Korea Standard Time", "Asia/Pyongyang" },
                
                // 南亞和西亞
                { "India Standard Time", "Asia/Kolkata" },
                { "Sri Lanka Standard Time", "Asia/Colombo" },
                { "Nepal Standard Time", "Asia/Kathmandu" },
                { "Bangladesh Standard Time", "Asia/Dhaka" },
                { "Pakistan Standard Time", "Asia/Karachi" },
                { "Afghanistan Standard Time", "Asia/Kabul" },
                { "Arabian Standard Time", "Asia/Dubai" },
                { "Arab Standard Time", "Asia/Riyadh" },
                { "Arabic Standard Time", "Asia/Baghdad" },
                { "Iran Standard Time", "Asia/Tehran" },
                { "Israel Standard Time", "Asia/Jerusalem" },
                { "Jordan Standard Time", "Asia/Amman" },
                { "Middle East Standard Time", "Asia/Beirut" },
                
                // 北美
                { "Eastern Standard Time", "America/New_York" },
                { "Central Standard Time", "America/Chicago" },
                { "Mountain Standard Time", "America/Denver" },
                { "Pacific Standard Time", "America/Los_Angeles" },
                { "Alaskan Standard Time", "America/Anchorage" },
                { "US Mountain Standard Time", "America/Phoenix" },
                { "US Eastern Standard Time", "America/Indianapolis" },
                { "Atlantic Standard Time", "America/Halifax" },
                { "Newfoundland Standard Time", "America/St_Johns" },
                
                // 墨西哥和中美洲
                { "Central Standard Time (Mexico)", "America/Mexico_City" },
                { "Mountain Standard Time (Mexico)", "America/Chihuahua" },
                { "Central America Standard Time", "America/Guatemala" },
                
                // 南美
                { "SA Pacific Standard Time", "America/Bogota" },
                { "Venezuela Standard Time", "America/Caracas" },
                { "Pacific SA Standard Time", "America/Santiago" },
                { "Argentina Standard Time", "America/Buenos_Aires" },
                { "E. South America Standard Time", "America/Sao_Paulo" },
                { "UTC-02", "America/Noronha" },
                
                // 歐洲
                { "GMT Standard Time", "Europe/London" },
                { "Romance Standard Time", "Europe/Paris" },
                { "W. Europe Standard Time", "Europe/Berlin" },
                { "GTB Standard Time", "Europe/Bucharest" },
                { "FLE Standard Time", "Europe/Helsinki" },
                { "Turkey Standard Time", "Europe/Istanbul" },
                { "Russian Standard Time", "Europe/Moscow" },
                { "Central European Standard Time", "Europe/Warsaw" },
                
                // 澳洲和紐西蘭
                { "AUS Eastern Standard Time", "Australia/Sydney" },
                { "E. Australia Standard Time", "Australia/Brisbane" },
                { "Cen. Australia Standard Time", "Australia/Adelaide" },
                { "AUS Central Standard Time", "Australia/Darwin" },
                { "W. Australia Standard Time", "Australia/Perth" },
                { "New Zealand Standard Time", "Pacific/Auckland" },
                { "Fiji Standard Time", "Pacific/Fiji" },
                
                // 太平洋區域
                { "Hawaiian Standard Time", "Pacific/Honolulu" },
                { "West Pacific Standard Time", "Pacific/Guam" },
                { "Samoa Standard Time", "Pacific/Samoa" },
                { "Central Pacific Standard Time", "Pacific/Guadalcanal" },
                
                // 非洲
                { "Egypt Standard Time", "Africa/Cairo" },
                { "South Africa Standard Time", "Africa/Johannesburg" },
                { "W. Central Africa Standard Time", "Africa/Lagos" },
                { "E. Africa Standard Time", "Africa/Nairobi" },
                { "Morocco Standard Time", "Africa/Casablanca" },
                { "Libya Standard Time", "Africa/Tripoli" },
                
                // UTC
                { "UTC", "Etc/UTC" }
            };

            // 根據指定的IDType或當前系統進行映射
            bool useWindowsFormat = IDType == "Windows" || (IDType == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            bool useLinuxFormat = IDType == "Linux" || (IDType == null && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

            if (useWindowsFormat)
            {
                // 將 Linux 格式轉換為 Windows 格式
                if (linuxToWindowsMap.ContainsKey(timeZoneId))
                {
                    return linuxToWindowsMap[timeZoneId];
                }
                // 如果已經是 Windows 格式或未知格式，直接返回
                return timeZoneId;
            }
            else if (useLinuxFormat)
            {
                // 將 Windows 格式轉換為 Linux 格式
                if (windowsToLinuxMap.ContainsKey(timeZoneId))
                {
                    return windowsToLinuxMap[timeZoneId];
                }
                // 如果已經是 Linux 格式或未知格式，直接返回
                return timeZoneId;
            }
            else
            {
                // 未指定有效的IDType，依照當前系統
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
                    (linuxToWindowsMap.ContainsKey(timeZoneId) ? linuxToWindowsMap[timeZoneId] : timeZoneId) : 
                    (windowsToLinuxMap.ContainsKey(timeZoneId) ? windowsToLinuxMap[timeZoneId] : timeZoneId);
            }
        }

        /// <summary>
        /// 獲取使用者時區的時間偏移量
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int GetUserTimeZoneOffset()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new AppException("Cannot access HTTP context!");
            }

            // 從 JWT Token Claims 中取得 timeZoneId
            var timeZoneIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "timeZoneId");
            string timeZoneId = MapTimeZoneId(timeZoneIdClaim?.Value);

            // 找到對應的時區
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // 獲取當前日期時間
            DateTime now = DateTime.UtcNow;

            // 計算時區偏移量（以小時為單位）
            TimeSpan offset = userTimeZone.GetUtcOffset(now);

            // 返回偏移的小時數
            return (int)offset.TotalHours;
        }
    }
}

