using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server;
using OpenFeature;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace Services.Shared.FeatureFlag.Providers;

/// <summary>
/// LaunchDarkly Provider for OpenFeature
/// </summary>
public class LaunchDarklyProvider : FeatureProvider
{
    private readonly LdClient _client;

    public LaunchDarklyProvider(LdClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public override Metadata GetMetadata()
    {
        return new Metadata("LaunchDarkly");
    }

    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var ldContext = ConvertContext(context);
        var value = _client.BoolVariation(flagKey, ldContext, defaultValue);

        return Task.FromResult(new ResolutionDetails<bool>(
            flagKey: flagKey,
            value: value,
            reason: Reason.TargetingMatch
        ));
    }

    public override Task<ResolutionDetails<string>> ResolveStringValueAsync(
        string flagKey,
        string defaultValue,
        EvaluationContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var ldContext = ConvertContext(context);
        var value = _client.StringVariation(flagKey, ldContext, defaultValue);

        return Task.FromResult(new ResolutionDetails<string>(
            flagKey: flagKey,
            value: value,
            reason: Reason.TargetingMatch
        ));
    }

    public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(
        string flagKey,
        int defaultValue,
        EvaluationContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var ldContext = ConvertContext(context);
        var value = _client.IntVariation(flagKey, ldContext, defaultValue);

        return Task.FromResult(new ResolutionDetails<int>(
            flagKey: flagKey,
            value: value,
            reason: Reason.TargetingMatch
        ));
    }

    public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(
        string flagKey,
        double defaultValue,
        EvaluationContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var ldContext = ConvertContext(context);
        var value = _client.DoubleVariation(flagKey, ldContext, defaultValue);

        return Task.FromResult(new ResolutionDetails<double>(
            flagKey: flagKey,
            value: value,
            reason: Reason.TargetingMatch
        ));
    }

    public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(
        string flagKey,
        Value defaultValue,
        EvaluationContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var ldContext = ConvertContext(context);
        var ldValue = _client.JsonVariation(flagKey, ldContext, LdValue.Null);

        var value = ConvertLdValueToOpenFeatureValue(ldValue);

        return Task.FromResult(new ResolutionDetails<Value>(
            flagKey: flagKey,
            value: value ?? defaultValue,
            reason: Reason.TargetingMatch
        ));
    }

    /// <summary>
    /// Â∞?OpenFeature EvaluationContext ËΩâÊ???LaunchDarkly Context
    /// </summary>
    private Context ConvertContext(EvaluationContext? context)
    {
        if (context == null)
        {
            return Context.New("anonymous-user");
        }

        // Âæ?context ‰∏≠Â?Âæ?targetingKey
        var targetingKey = context.GetValue("targetingKey")?.AsString ?? "anonymous-user";
        
        var builder = Context.Builder(targetingKey);

        // Ê∑ªÂ??∂‰?Â±¨ÊÄ?
        foreach (var (key, value) in context)
        {
            if (key == "targetingKey") continue; // Â∑≤Á?Ë®≠Â??é‰?

            if (value.IsString)
            {
                builder.Set(key, value.AsString!);
            }
            else if (value.IsNumber)
            {
                builder.Set(key, value.AsDouble!.Value);
            }
            else if (value.IsBoolean)
            {
                builder.Set(key, value.AsBoolean!.Value);
            }
        }

        return builder.Build();
    }

    /// <summary>
    /// Â∞?LaunchDarkly LdValue ËΩâÊ???OpenFeature Value
    /// </summary>
    private Value? ConvertLdValueToOpenFeatureValue(LdValue ldValue)
    {
        if (ldValue.IsNull)
        {
            return new Value();
        }

        if (ldValue.IsString)
        {
            return new Value(ldValue.AsString);
        }

        if (ldValue.IsNumber)
        {
            if (ldValue.IsInt)
                return new Value(ldValue.AsInt);
            return new Value(ldValue.AsDouble);
        }

        if (ldValue.Type == LdValueType.Bool)
        {
            return new Value(ldValue.AsBool);
        }

        // Â∞çÊñºË§áÈ??ÑÁâ©‰ª∂Ô??ûÂÇ≥ null ‰ΩøÁî®?êË®≠??
        return null;
    }
}

