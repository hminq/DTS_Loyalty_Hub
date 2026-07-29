using Campaign.Contracts.Definitions;
using Consumer.Core.Abstractions;
using Consumer.Core.Exceptions;
using Consumer.Core.Entities.Constants;

namespace Consumer.Core.Services;

public sealed class CampaignEventRuntimeRegistry : ICampaignEventRuntimeRegistry
{
    private readonly IReadOnlyDictionary<string, ICampaignEventRuntimeDefinition> _runtimes;

    public CampaignEventRuntimeRegistry(
        CampaignDefinitionCatalog catalog,
        IEnumerable<ICampaignEventRuntimeDefinition> runtimeDefinitions)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(runtimeDefinitions);

        var runtimes = runtimeDefinitions.ToArray();
        var duplicate = runtimes
            .GroupBy(runtime => runtime.EventType, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Duplicate campaign event runtime registration: '{duplicate.Key}'.");
        }

        _runtimes = runtimes.ToDictionary(
            runtime => runtime.EventType,
            StringComparer.Ordinal);

        var catalogEventTypes = catalog.Events
            .Select(definition => definition.Code)
            .ToHashSet(StringComparer.Ordinal);
        var runtimeEventTypes = _runtimes.Keys.ToHashSet(StringComparer.Ordinal);

        var missingRuntimes = catalogEventTypes
            .Except(runtimeEventTypes, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var unknownRuntimes = runtimeEventTypes
            .Except(catalogEventTypes, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (missingRuntimes.Length > 0 || unknownRuntimes.Length > 0)
        {
            throw new InvalidOperationException(
                "Campaign event runtime registry does not match the definition catalog. " +
                $"Missing=[{string.Join(",", missingRuntimes)}]; " +
                $"Unknown=[{string.Join(",", unknownRuntimes)}].");
        }
    }

    public ICampaignEventRuntimeDefinition GetRequired(string? eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType) ||
            !_runtimes.TryGetValue(eventType, out var runtime))
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventTypeUnsupported);
        }

        return runtime;
    }
}
