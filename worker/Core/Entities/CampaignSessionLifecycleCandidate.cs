namespace Core.Entities;

public sealed record CampaignSessionLifecycleCandidate(
    Guid CampaignSessionId,
    Guid CampaignId,
    DateTime SessionStart,
    DateTime SessionEnd,
    string Status,
    DateTime? EndedAt);
