using Consumer.Core.Requests;
using Consumer.Core.Entities.Campaigns;

namespace Consumer.Core.Abstractions;

public interface ICampaignProcessingScopeExecutor
{
    Task<PrepareCampaignEventResult> PrepareEventAsync(
        IValidatedCampaignEvent campaignEvent,
        CancellationToken cancellationToken = default);

    Task<ProcessCampaignResult> ProcessCampaignAsync(
        Guid eventCampaignProcessingId,
        CancellationToken cancellationToken = default);

    Task<RecordCampaignProcessingFailureResult> RecordFailureAsync(
        Guid eventCampaignProcessingId,
        string outcomeCode,
        string errorSummary,
        CancellationToken cancellationToken = default);

    Task<FinalizeEventProcessingResult> FinalizeEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}
