using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface IEventProcessingFinalizationStore
{
    Task<CampaignProcessingFailureState?> LockCampaignProcessingAsync(
        Guid eventCampaignProcessingId,
        CancellationToken cancellationToken = default);

    void MarkCampaignFailed(
        Guid eventCampaignProcessingId,
        string outcomeCode,
        string errorSummary,
        DateTime processedAt);

    Task<EventProcessingFinalizationState?> LockEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    void MarkEventCompleted(Guid eventId, DateTime completedAt);

    void MarkEventFailed(Guid eventId, DateTime failedAt);
}
