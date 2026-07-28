using Core.Requests;

namespace Core.Entities.Campaigns;

public sealed record CampaignProcessingSequenceResult(
    Guid EventId,
    IReadOnlyList<ProcessCustomerRegistrationCampaignResult> CampaignResults,
    IReadOnlyList<RecordCampaignProcessingFailureResult> FailureResults,
    FinalizeEventProcessingResult Finalization);
