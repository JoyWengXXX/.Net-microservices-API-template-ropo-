using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Common.Helpers.Interfaces;
using System.Net.Http.Headers;
using System.Text;

namespace Service.Common.Helpers
{
    /// <summary>
    /// HTTP è«‹æ?è¼”åŠ©å·¥å…·é¡?    /// </summary>
    public class HttpClientRequestHelper : IHttpClientRequestHelper
    {
        private readonly ILogger<HttpClientRequestHelper> _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// å»ºæ??½æ•¸
        /// </summary>
        /// <param name="logger">?¥è?è¨˜é???/param>
        /// <param name="httpClient">HTTP å®¢æˆ¶ç«?/param>
        public HttpClientRequestHelper(ILogger<HttpClientRequestHelper> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// ?·è? GET è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        public async Task<HttpResponseMessage> GetAsync(string url, string token = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            ConfigureRequest(request, token);

            _logger.LogInformation($"GET è«‹æ??‹å?: {url}");
            return await _httpClient.SendAsync(request);
        }

        /// <summary>
        /// ?·è? POST è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="data">è«‹æ??§å®¹</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        public async Task<HttpResponseMessage> PostAsync(string url, object data, string token = null)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            ConfigureRequest(request, token);

            _logger.LogInformation($"POST è«‹æ??‹å?: {url}, è³‡æ?: {json}");
            return await _httpClient.SendAsync(request);
        }

        /// <summary>
        /// ?·è? PUT è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="data">è«‹æ??§å®¹</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        public async Task<HttpResponseMessage> PutAsync(string url, object data, string token = null)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };
            ConfigureRequest(request, token);

            _logger.LogInformation($"PUT è«‹æ??‹å?: {url}, è³‡æ?: {json}");
            return await _httpClient.SendAsync(request);
        }

        /// <summary>
        /// ?·è? DELETE è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="data">è«‹æ??§å®¹</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        public async Task<HttpResponseMessage> DeleteAsync(string url, object data, string token = null)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = content
            };
            ConfigureRequest(request, token);

            _logger.LogInformation($"DELETE è«‹æ??‹å?: {url}, è³‡æ?: {json}");
            return await _httpClient.SendAsync(request);
        }

        /// <summary>
        /// ?·è? PATCH è«‹æ?
        /// </summary>
        /// <param name="url">API ç¶²å?</param>
        /// <param name="data">è«‹æ??§å®¹</param>
        /// <param name="token">èªè? Token (?¯é¸)</param>
        /// <returns>API ?æ?è³‡æ?</returns>
        public async Task<HttpResponseMessage> PatchAsync(string url, object data, string token = null)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = content
            };
            ConfigureRequest(request, token);

            _logger.LogInformation($"PATCH è«‹æ??‹å?: {url}, è³‡æ?: {json}");
            return await _httpClient.SendAsync(request);
        }

        /// <summary>
        /// è¨­ç½®è«‹æ?æ¨™é ­
        /// </summary>
        private void ConfigureRequest(HttpRequestMessage request, string token)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}

