namespace Consumer.Core.Entities.Campaigns;

public sealed record EventPreparationState(
    Guid EventId,
    string EventType,
    string RoutingKey,
    DateTime OccurredAt,
    string PayloadHash,
    string Status,
    IReadOnlyList<PreparedCampaignTarget> Targets);
