using Services.Shared.FeatureFlag.Interfaces;
using Services.Shared.FeatureFlag.Models;
using Services.Shared.FeatureFlag.Attributes;
using System.Reflection;

namespace Services.Shared.FeatureFlag.Helpers
{
    /// <summary>
    /// Feature Flag è·¯ç”±å¹«åŠ©é¡?
    /// </summary>
    public static class FeatureFlagHelper
    {
        /// <summary>
        /// ?¹æ??¹æ?ä¸Šç? FeatureFlagAttribute ?ªå?è·¯ç”±
        /// </summary>
        public static async Task<TResult> RouteByAttribute<TQuery, TResult>(
            object handler,
            IFeatureFlagService featureFlagService,
            TQuery query,
            string? userId = null,
            string callerMethodName = null)
        {
            // ?²å?èª¿ç”¨æ­¤æ–¹æ³•ç??¹æ?ï¼ˆå³æ¨™è?äº?Attribute ?„æ–¹æ³•ï?
            var handlerType = handler.GetType();
            
            // ä½¿ç”¨?ƒæ•¸é¡å?ç²¾ç¢ºå®šä??¹æ?ï¼Œé¿??AmbiguousMatchException
            var method = handlerType.GetMethod(
                callerMethodName ?? "HandleAsync", 
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(TQuery) },
                null);

            if (method == null)
            {
                throw new InvalidOperationException($"Method '{callerMethodName}' with parameter type '{typeof(TQuery).Name}' not found");
            }

            // ?²å? FeatureFlagAttribute
            var attribute = method.GetCustomAttribute<FeatureFlagAttribute>();
            if (attribute == null)
            {
                throw new InvalidOperationException($"Method '{method.Name}' does not have FeatureFlagAttribute");
            }

            // ä½¿ç”¨ attribute ?„é?ç½®é€²è?è·¯ç”±
            return await RouteByFeatureFlag<TQuery, TResult>(
                handler,
                featureFlagService,
                attribute.FlagName,
                userId,
                query,
                attribute.UnifiedMethodName,
                attribute.LegacyMethodName
            );
        }

        /// <summary>
        /// ?¹æ? Feature Flag è·¯ç”±?°å??‰ç??¹æ?
        /// </summary>
        public static async Task<TResult> RouteByFeatureFlag<TQuery, TResult>(
            object handler,
            IFeatureFlagService featureFlagService,
            string flagName,
            string? userId = null,
            TQuery query = default,
            string unifiedMethodName = null,
            string legacyMethodName = null)
        {
            // ?µå»º Feature Flag ä¸Šä???
            var featureContext = new FeatureFlagContext
            {
                Environment = "production"
            };

            // ?ªæ???userId ?‰å€¼æ??è¨­ç½®ï??¦å?ä½¿ç”¨ LaunchDarkly ??default rule
            if (!string.IsNullOrWhiteSpace(userId))
            {
                featureContext.UserId = userId;
            }

            // æª¢æŸ¥ Feature Flag
            bool useUnifiedService = await featureFlagService.IsEnabledAsync(flagName, featureContext);

            // ?¹æ? Flag ?¸æ??¹æ?
            string methodName = useUnifiedService ? unifiedMethodName : legacyMethodName;

            // ä½¿ç”¨?å?èª¿ç”¨å°æ??¹æ?
            var method = handler.GetType().GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
            );

            if (method == null)
            {
                throw new InvalidOperationException($"Method '{methodName}' not found in {handler.GetType().Name}");
            }

            var task = method.Invoke(handler, new object[] { query }) as Task<TResult>;
            if (task == null)
            {
                throw new InvalidOperationException($"Method '{methodName}' did not return Task<{typeof(TResult).Name}>");
            }

            return await task;
        }
    }
}

