namespace Consumer.Core.Entities.Campaigns;

public sealed record SuccessfulActionExecutionMutation(
    Guid CampaignUsageId,
    Guid ActionUsageId,
    Guid EventCampaignProcessingId,
    Guid CampaignId,
    Guid CampaignSessionId,
    Guid CustomerId,
    Guid ActionId,
    DateTime CreatedAt);
