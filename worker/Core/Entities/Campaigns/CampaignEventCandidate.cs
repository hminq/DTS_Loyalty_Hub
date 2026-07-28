namespace Core.Entities.Campaigns;

public sealed record CampaignEventCandidate(
    Guid CampaignId,
    Guid CampaignSessionId);
