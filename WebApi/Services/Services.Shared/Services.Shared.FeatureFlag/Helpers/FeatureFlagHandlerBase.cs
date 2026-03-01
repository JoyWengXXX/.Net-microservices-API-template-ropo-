using Services.Shared.FeatureFlag.Interfaces;
using Services.Shared.FeatureFlag.Models;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Services.Shared.FeatureFlag.Helpers
{
    /// <summary>
    /// ?ØÊè¥ Feature Flag ?ÑÂü∫È°?
    /// </summary>
    public abstract class FeatureFlagHandlerBase
    {
        protected readonly IFeatureFlagService _featureFlagService;

        protected FeatureFlagHandlerBase(IFeatureFlagService featureFlagService)
        {
            _featureFlagService = featureFlagService;
        }

        /// <summary>
        /// ?πÊ??πÊ?‰∏äÁ? FeatureFlagAttribute ?™Â?Ë∑ØÁî±Ôºà‰Ωø?®ÂñÆ‰∏Ä?πÊ? + ?ØÈÅ∏?ÉÊï∏Ôº?
        /// </summary>
        protected async Task<TResult> RouteByAttribute<TQuery, TResult>(
            TQuery query,
            string userId = null,
            [CallerMemberName] string callerMethodName = null)
        {
            return await FeatureFlagHelper.RouteByAttribute<TQuery, TResult>(
                handler: this,
                featureFlagService: _featureFlagService,
                query: query,
                userId: userId,
                callerMethodName: callerMethodName
            );
        }

        /// <summary>
        /// ?πÊ? Feature Flag ?ãÂ?Ë∑ØÁî±
        /// </summary>
        protected async Task<TResult> RouteByFeatureFlag<TQuery, TResult>(
            string flagName,
            string userId,
            TQuery query,
            string unifiedMethodName,
            string legacyMethodName)
        {
            return await FeatureFlagHelper.RouteByFeatureFlag<TQuery, TResult>(
                this,
                _featureFlagService,
                flagName,
                userId,
                query,
                unifiedMethodName,
                legacyMethodName
            );
        }
    }
}

