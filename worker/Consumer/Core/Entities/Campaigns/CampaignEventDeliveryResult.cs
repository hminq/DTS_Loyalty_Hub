namespace Consumer.Core.Entities.Campaigns;

public sealed record CampaignEventDeliveryResult(
    string Disposition,
    Guid? EventId,
    string? ErrorCode,
    int PinnedCampaignCount,
    int CompletedCampaignCount,
    int SkippedCampaignCount,
    int FailedCampaignCount);
