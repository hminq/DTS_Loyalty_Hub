namespace Consumer.Core.Entities.Campaigns;

public sealed record CampaignTargetResolution(
    string Status,
    string TargetKind,
    Guid? TargetId,
    string? ErrorCode);
