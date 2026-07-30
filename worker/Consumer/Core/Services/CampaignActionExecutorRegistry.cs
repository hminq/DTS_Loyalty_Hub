using Campaign.Contracts.Definitions;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;

namespace Consumer.Core.Services;

public sealed class CampaignActionExecutorRegistry : ICampaignActionExecutorRegistry
{
    private readonly IReadOnlyDictionary<string, ICampaignActionExecutor> _executors;

    public CampaignActionExecutorRegistry(
        CampaignActionCatalog actionCatalog,
        IEnumerable<ICampaignActionExecutor> executors)
    {
        ArgumentNullException.ThrowIfNull(actionCatalog);
        ArgumentNullException.ThrowIfNull(executors);

        var registered = executors.ToArray();
        var duplicate = registered
            .GroupBy(executor => executor.ActionType, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Duplicate campaign action executor registration: '{duplicate.Key}'.");
        }

        _executors = registered.ToDictionary(
            executor => executor.ActionType,
            StringComparer.Ordinal);

        var catalogActions = actionCatalog.Actions.ToDictionary(
            definition => definition.Code,
            StringComparer.Ordinal);
        var missingExecutors = catalogActions.Keys
            .Except(_executors.Keys, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var unknownExecutors = _executors.Keys
            .Except(catalogActions.Keys, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var incompatibleExecutors = registered
            .Where(executor =>
                catalogActions.TryGetValue(executor.ActionType, out var definition) &&
                !string.Equals(
                    executor.RequiredTargetKind,
                    definition.RequiredTargetKind,
                    StringComparison.Ordinal))
            .Select(executor => executor.ActionType)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (missingExecutors.Length > 0 ||
            unknownExecutors.Length > 0 ||
            incompatibleExecutors.Length > 0)
        {
            throw new InvalidOperationException(
                "Campaign action executor registry does not match the definition catalog. " +
                $"Missing=[{string.Join(",", missingExecutors)}]; " +
                $"Unknown=[{string.Join(",", unknownExecutors)}]; " +
                $"Incompatible=[{string.Join(",", incompatibleExecutors)}].");
        }
    }

    public ICampaignActionExecutor GetRequired(string actionType)
    {
        if (string.IsNullOrWhiteSpace(actionType) ||
            !_executors.TryGetValue(actionType, out var executor))
        {
            throw new CampaignConfigurationException(
                CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
        }

        return executor;
    }
}
