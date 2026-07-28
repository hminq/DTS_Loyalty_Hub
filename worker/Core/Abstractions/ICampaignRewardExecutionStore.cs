using Core.Entities.Campaigns;

namespace Core.Abstractions;

public interface ICampaignRewardExecutionStore
{
    Task<CampaignRewardProcessingContext?> LockProcessingAsync(
        Guid eventCampaignProcessingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> LockCustomersAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default);

    Task<CampaignUsageCounts> GetCampaignUsageCountsAsync(
        Guid campaignId,
        Guid campaignSessionId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignRewardAction>> LockActionsAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task LockCustomerPointsAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> LockActionUsagesAsync(
        IReadOnlyCollection<Guid> actionIds,
        Guid campaignSessionId,
        CancellationToken cancellationToken = default);

    void ApplyPointReward(PointRewardMutation mutation);

    void MarkSkipped(
        Guid eventCampaignProcessingId,
        string outcomeCode,
        DateTime processedAt);

    void MarkCompleted(
        Guid eventCampaignProcessingId,
        DateTime processedAt);
}
