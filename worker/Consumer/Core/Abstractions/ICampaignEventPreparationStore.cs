using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface ICampaignEventPreparationStore
{
    Task<bool> TryInsertEventAsync(
        IValidatedCampaignEvent campaignEvent,
        DateTime createdAt,
        CancellationToken cancellationToken = default);

    Task<EventPreparationState?> GetEventStateAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignEventCandidate>> GetCandidateTargetsAsync(
        Guid eventTypeVersionId,
        DateTime occurredAt,
        CancellationToken cancellationToken = default);

    void AddTargets(
        Guid eventId,
        IReadOnlyList<PreparedCampaignTarget> targets,
        DateTime createdAt);

    Task MarkEventCompletedAsync(
        Guid eventId,
        DateTime completedAt,
        CancellationToken cancellationToken = default);
}
