
namespace Service.Common.Helpers.Interfaces
{
    /// <summary>
    /// HTTP è«‹æ?è¼”åŠ©å·¥å…·ä»‹é¢
    /// </summary>
    public interface IHttpClientRequestHelper
    {
        /// <summary>
        /// ?·è? GET è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        Task<HttpResponseMessage> GetAsync(string url, string token = null);

        /// <summary>
        /// ?·è? POST è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="data">è«‹æ??§å®¹</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        Task<HttpResponseMessage> PostAsync(string url, object data, string token = null);

        /// <summary>
        /// ?·è? PUT è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="data">è«‹æ??§å®¹</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        Task<HttpResponseMessage> PutAsync(string url, object data, string token = null);

        /// <summary>
        /// ?·è? DELETE è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="data">è«‹æ??§å®¹</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        Task<HttpResponseMessage> DeleteAsync(string url, object data, string token = null);

        /// <summary>
        /// ?·è? PATCH è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="data">è«‹æ??§å®¹</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        Task<HttpResponseMessage> PatchAsync(string url, object data, string token = null);
    }
}

