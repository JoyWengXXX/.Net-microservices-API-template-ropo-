using Microsoft.Extensions.DependencyInjection;
using Service.Common.Helpers.Interfaces;

namespace Service.Common.Helpers
{
    /// <summary>
    /// HttpClientRequestHelper ?„æ“´å±•æ–¹æ³?    /// </summary>
    public static class HttpClientRequestHelperExtensions
    {
        /// <summary>
        /// å°?HttpClientRequestHelper è¨»å??°æ??™å®¹??        /// æ­¤æ“´å±•æ–¹æ³•æ??ªå??Œæ?è¨»å? HttpClient
        /// </summary>
        /// <param name="services">?å?å®¹å™¨</param>
        /// <returns>?å?å®¹å™¨</returns>
        public static IServiceCollection AddHttpClientRequestHelper(this IServiceCollection services)
        {
            // è¨»å? HttpClient
            services.AddHttpClient();
            
            // è¨»å? HttpClientRequestHelper
            services.AddScoped<IHttpClientRequestHelper, HttpClientRequestHelper>();
            
            return services;
        }
    }
}
