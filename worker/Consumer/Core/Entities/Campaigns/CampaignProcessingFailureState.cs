namespace Consumer.Core.Entities.Campaigns;

public sealed record CampaignProcessingFailureState(
    Guid EventCampaignProcessingId,
    string Status,
    int AttemptCount,
    string? OutcomeCode);
