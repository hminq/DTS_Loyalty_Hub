using Scheduler.Core.Entities;

namespace Scheduler.Core.Abstractions;

public interface ICampaignSessionLifecycleStore
{
    Task<IReadOnlyList<CampaignSessionLifecycleCandidate>> GetDueScheduledSessionsForUpdateAsync(
        DateTime processedAt,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignSessionLifecycleCandidate>> GetDueRunningSessionsForUpdateAsync(
        DateTime processedAt,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task ApplySessionMutationsAsync(
        IReadOnlyList<CampaignSessionLifecycleMutation> mutations,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetEligibleCampaignsForUpdateAsync(
        DateTime processedAt,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task EndCampaignsAsync(
        IReadOnlyList<Guid> campaignIds,
        DateTime processedAt,
        CancellationToken cancellationToken = default);
}
