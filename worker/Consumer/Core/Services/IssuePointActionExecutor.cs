using Campaign.Contracts.Actions;
using Campaign.Contracts.Constants;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;

namespace Consumer.Core.Services;

public sealed class IssuePointActionExecutor : ICampaignActionExecutor
{
    private readonly IIssuePointExecutionStore _store;

    public IssuePointActionExecutor(IIssuePointExecutionStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public string ActionType => ActionTypes.IssuePoint;

    public string RequiredTargetKind => CampaignTargetKinds.Customer;

    public Task PrepareAsync(
        IReadOnlyCollection<ResolvedCampaignAction> actions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var customerIds = actions
            .Select(action => action.TargetId)
            .Distinct()
            .Order()
            .ToArray();

        return _store.PrepareIssuePointExecutionAsync(customerIds, cancellationToken);
    }

    public void Execute(
        ResolvedCampaignAction action,
        CampaignActionExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(context);

        if (action.ActionType != ActionType ||
            action.TargetKind != RequiredTargetKind ||
            action.ParsedParameters is not IssuePointParameters parameters)
        {
            throw new CampaignConfigurationException(
                CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
        }

        _store.ApplyIssuePoint(
            new IssuePointMutation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                context.EventId,
                context.CampaignId,
                context.CampaignSessionId,
                action.TargetId,
                action.ActionId,
                parameters.Amount,
                context.ExecutedAt));
    }
}
