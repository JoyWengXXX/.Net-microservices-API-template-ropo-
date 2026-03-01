using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Services.Shared.FeatureFlag.Interfaces;
using Services.Shared.FeatureFlag.Providers;
using OpenFeature;
using LaunchDarkly.Sdk.Server;

namespace Services.Shared.FeatureFlag.Extensions;

/// <summary>
/// DI ÂÆπÂô®?¥Â??πÊ?ÔºàÂü∫??OpenFeature + LaunchDarklyÔº?
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ë®ªÂ? Feature Flag ?çÂ?Ôºà‰Ωø??LaunchDarkly ProviderÔº?
    /// </summary>
    /// <param name="services">?çÂ??ÜÂ?</param>
    /// <param name="configuration">?çÁΩÆ</param>
    /// <returns>?çÂ??ÜÂ?</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // ËÆÄ??LaunchDarkly SDK KeyÔºàÂÑ™?àÈ?Â∫èÔ??∞Â?ËÆäÊï∏ > User Secrets > appsettingsÔº?
        var sdkKey = configuration["LAUNCHDARKLY_SDK_KEY"] 
                     ?? configuration["LaunchDarkly:SdkKey"];

        if (string.IsNullOrWhiteSpace(sdkKey))
        {
            throw new InvalidOperationException(
                "LaunchDarkly SDK Key is not configured. " +
                "Please set it via environment variable 'LAUNCHDARKLY_SDK_KEY', " +
                "User Secrets 'LaunchDarkly:SdkKey', " +
                "or appsettings 'LaunchDarkly:SdkKey'.");
        }

        // Âª∫Á? LaunchDarkly Client
        var ldConfig = Configuration.Default(sdkKey);
        var ldClient = new LdClient(ldConfig);

        // Ë®≠ÁΩÆ OpenFeature Provider
        var provider = new LaunchDarklyProvider(ldClient);
        Api.Instance.SetProviderAsync(provider).GetAwaiter().GetResult();

        // Ë®ªÂ??™Â?Áæ©Á? IFeatureFlagServiceÔºàÂ?Ë£?OpenFeature ClientÔº?
        services.TryAddSingleton<IFeatureFlagService, FeatureFlagService>();

        return services;
    }

    /// <summary>
    /// Ë®ªÂ? Feature Flag ?çÂ?Ôºà‰Ωø?®Ëá™Ë®?LaunchDarkly ?çÁΩÆÔº?
    /// </summary>
    /// <param name="services">?çÂ??ÜÂ?</param>
    /// <param name="sdkKey">LaunchDarkly SDK Key</param>
    /// <param name="configureClient">?çÁΩÆ LaunchDarkly Client ?ÑÂ?Ë™øÔ??ØÈÅ∏Ôº?/param>
    /// <returns>?çÂ??ÜÂ?</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        string sdkKey,
        Action<LaunchDarkly.Sdk.Server.ConfigurationBuilder>? configureClient = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (string.IsNullOrWhiteSpace(sdkKey))
            throw new ArgumentException("SDK Key cannot be null or empty", nameof(sdkKey));

        // ?çÁΩÆ LaunchDarkly Client
        var ldConfigBuilder = Configuration.Builder(sdkKey);

        // ?ÅË®±?™Ë??çÁΩÆ
        configureClient?.Invoke(ldConfigBuilder);

        var ldClient = new LdClient(ldConfigBuilder.Build());

        // Ë®≠ÁΩÆ OpenFeature Provider
        var provider = new LaunchDarklyProvider(ldClient);
        Api.Instance.SetProviderAsync(provider).GetAwaiter().GetResult();

        // Ë®ªÂ??™Â?Áæ©Á? IFeatureFlagService
        services.TryAddSingleton<IFeatureFlagService, FeatureFlagService>();

        return services;
    }

    /// <summary>
    /// Ë®ªÂ? Feature Flag ?çÂ?Ôºà‰Ωø?®Ëá™Ë®?OpenFeature ProviderÔº?
    /// </summary>
    /// <param name="services">?çÂ??ÜÂ?</param>
    /// <param name="provider">?™Ë???OpenFeature Provider</param>
    /// <returns>?çÂ??ÜÂ?</returns>
    public static IServiceCollection AddFeatureFlagsWithCustomProvider(
        this IServiceCollection services,
        FeatureProvider provider)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        // Ë®≠ÁΩÆ?™Ë? Provider
        Api.Instance.SetProviderAsync(provider).GetAwaiter().GetResult();

        // Ë®ªÂ??™Â?Áæ©Á? IFeatureFlagService
        services.TryAddSingleton<IFeatureFlagService, FeatureFlagService>();

        return services;
    }
}

