namespace Core.Entities.Campaigns;

public sealed record PointRewardMutation(
    Guid CustomerPointId,
    Guid PointTransactionId,
    Guid CampaignUsageId,
    Guid ActionUsageId,
    Guid EventCampaignProcessingId,
    Guid EventId,
    Guid CampaignId,
    Guid CampaignSessionId,
    Guid CustomerId,
    Guid ActionId,
    decimal Amount,
    DateTime CreatedAt);
