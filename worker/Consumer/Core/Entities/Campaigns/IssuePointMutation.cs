namespace Consumer.Core.Entities.Campaigns;

public sealed record IssuePointMutation(
    Guid CustomerPointId,
    Guid PointTransactionId,
    Guid EventId,
    Guid CampaignId,
    Guid CampaignSessionId,
    Guid CustomerId,
    Guid ActionId,
    decimal Amount,
    DateTime CreatedAt);
