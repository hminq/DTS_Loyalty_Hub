namespace Consumer.Core.Entities.Campaigns;

public sealed record CampaignActionExecutionContext(
    Guid EventCampaignProcessingId,
    Guid EventId,
    Guid CampaignId,
    Guid CampaignSessionId,
    DateTime ExecutedAt);
