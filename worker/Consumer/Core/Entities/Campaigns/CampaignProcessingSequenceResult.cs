using Consumer.Core.Requests;

namespace Consumer.Core.Entities.Campaigns;

public sealed record CampaignProcessingSequenceResult(
    Guid EventId,
    IReadOnlyList<ProcessCampaignResult> CampaignResults,
    IReadOnlyList<RecordCampaignProcessingFailureResult> FailureResults,
    FinalizeEventProcessingResult Finalization);
