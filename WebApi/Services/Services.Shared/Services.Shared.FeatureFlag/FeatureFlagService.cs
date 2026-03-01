using Microsoft.Extensions.Logging;
using Services.Shared.FeatureFlag.Interfaces;
using Services.Shared.FeatureFlag.Models;
using OpenFeature;
using OpenFeature.Model;

namespace Services.Shared.FeatureFlag;

/// <summary>
/// Feature Flag ?å?å¯¦ç¾ï¼ˆåŸº??OpenFeature æ¨™æ?ï¼?
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly ILogger<FeatureFlagService> _logger;
    private readonly FeatureClient _featureClient;

    public FeatureFlagService(
        ILogger<FeatureFlagService> logger)
    {
        _logger = logger;
        // ä½¿ç”¨ OpenFeature API ?²å? Client
        _featureClient = Api.Instance.GetClient();
    }

    public async Task<bool> IsEnabledAsync(string featureName, FeatureFlagContext? context = null)
    {
        return await GetBooleanValueAsync(featureName, false, context);
    }

    public async Task<bool> GetBooleanValueAsync(string featureName, bool defaultValue, FeatureFlagContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));
        }

        try
        {
            var evaluationContext = BuildEvaluationContext(context);
            var result = await _featureClient.GetBooleanValueAsync(featureName, defaultValue, evaluationContext);
            _logger.LogDebug("Evaluated boolean feature '{FeatureName}' => {Result}", featureName, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate feature '{FeatureName}', returning default value {DefaultValue}", featureName, defaultValue);
            return defaultValue;
        }
    }

    public async Task<string> GetStringValueAsync(string featureName, string defaultValue, FeatureFlagContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));
        }

        try
        {
            var evaluationContext = BuildEvaluationContext(context);
            var result = await _featureClient.GetStringValueAsync(featureName, defaultValue, evaluationContext);
            _logger.LogDebug("Evaluated string feature '{FeatureName}' => {Result}", featureName, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get string value for feature '{FeatureName}', returning default value", featureName);
            return defaultValue;
        }
    }

    public async Task<int> GetIntegerValueAsync(string featureName, int defaultValue, FeatureFlagContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));
        }

        try
        {
            var evaluationContext = BuildEvaluationContext(context);
            var result = await _featureClient.GetIntegerValueAsync(featureName, defaultValue, evaluationContext);
            _logger.LogDebug("Evaluated integer feature '{FeatureName}' => {Result}", featureName, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get integer value for feature '{FeatureName}', returning default value", featureName);
            return defaultValue;
        }
    }

    public async Task<double> GetDoubleValueAsync(string featureName, double defaultValue, FeatureFlagContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));
        }

        try
        {
            var evaluationContext = BuildEvaluationContext(context);
            var result = await _featureClient.GetDoubleValueAsync(featureName, defaultValue, evaluationContext);
            _logger.LogDebug("Evaluated double feature '{FeatureName}' => {Result}", featureName, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get double value for feature '{FeatureName}', returning default value", featureName);
            return defaultValue;
        }
    }

    public async Task<Value> GetObjectValueAsync(string featureName, Value defaultValue, FeatureFlagContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));
        }

        try
        {
            var evaluationContext = BuildEvaluationContext(context);
            var result = await _featureClient.GetObjectValueAsync(featureName, defaultValue, evaluationContext);
            _logger.LogDebug("Evaluated object feature '{FeatureName}' => {Result}", featureName, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get object value for feature '{FeatureName}', returning default value", featureName);
            return defaultValue;
        }
    }

    public async Task<Dictionary<string, bool>> GetFeaturesStatusAsync(IEnumerable<string> featureNames, FeatureFlagContext? context = null)
    {
        if (featureNames == null)
        {
            throw new ArgumentNullException(nameof(featureNames));
        }

        var result = new Dictionary<string, bool>();

        // ä¸¦è??·è??¥è©¢
        var tasks = featureNames.Select(async featureName =>
        {
            var isEnabled = await IsEnabledAsync(featureName, context);
            return new KeyValuePair<string, bool>(featureName, isEnabled);
        });

        var results = await Task.WhenAll(tasks);
        
        foreach (var kvp in results)
        {
            result[kvp.Key] = kvp.Value;
        }

        return result;
    }

    /// <summary>
    /// å°‡è‡ªå®šç¾© FeatureFlagContext è½‰æ???OpenFeature EvaluationContext
    /// LaunchDarkly è¦æ?å¿…é??ä? targetingKey ??key
    /// </summary>
    private EvaluationContext BuildEvaluationContext(FeatureFlagContext? context)
    {
        var builder = EvaluationContext.Builder();

        if (context == null || context == FeatureFlagContext.Empty)
        {
            // LaunchDarkly è¦æ?å¿…é???keyï¼Œä½¿?¨é?è¨­ç??¿å??¨æˆ¶
            builder.Set("targetingKey", "anonymous-user");
            return builder.Build();
        }

        // ?ªå?ä½¿ç”¨ UserId ä½œç‚º targetingKeyï¼ˆLaunchDarkly ?„ä¸»è¦è??¥ï?
        if (!string.IsNullOrEmpty(context.UserId))
        {
            builder.Set("targetingKey", context.UserId);
        }
        else
        {
            // ?¥æ???UserIdï¼Œä½¿?¨åŒ¿?æ?è­?
            builder.Set("targetingKey", "anonymous-user");
        }

        // æ·»å??¶ä?ä¸Šä??‡å±¬??
        if (!string.IsNullOrEmpty(context.StoreId))
        {
            builder.Set("storeId", context.StoreId);
        }

        if (!string.IsNullOrEmpty(context.Environment))
        {
            builder.Set("environment", context.Environment);
        }

        // æ·»å??ªå?ç¾©å±¬??
        if (context.Properties != null)
        {
            foreach (var kvp in context.Properties)
            {
                builder.Set(kvp.Key, kvp.Value ?? string.Empty);
            }
        }

        return builder.Build();
    }
}

