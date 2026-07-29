using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface ICampaignActionExecutor
{
    string ActionType { get; }

    string RequiredTargetKind { get; }

    Task PrepareAsync(
        IReadOnlyCollection<ResolvedCampaignAction> actions,
        CancellationToken cancellationToken = default);

    void Execute(
        ResolvedCampaignAction action,
        CampaignActionExecutionContext context);
}
