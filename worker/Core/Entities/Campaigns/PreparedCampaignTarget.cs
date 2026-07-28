namespace Core.Entities.Campaigns;

public sealed record PreparedCampaignTarget(
    Guid EventCampaignProcessingId,
    Guid CampaignId,
    Guid CampaignSessionId,
    string Status);
