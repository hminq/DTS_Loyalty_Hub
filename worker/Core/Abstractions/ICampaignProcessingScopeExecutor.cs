using Core.Requests;
using Core.Entities.Campaigns;

namespace Core.Abstractions;

public interface ICampaignProcessingScopeExecutor
{
    Task<PrepareCustomerAccountRegisteredEventResult> PrepareEventAsync(
        ValidatedCustomerAccountRegisteredEvent campaignEvent,
        CancellationToken cancellationToken = default);

    Task<ProcessCustomerRegistrationCampaignResult> ProcessCampaignAsync(
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
