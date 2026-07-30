namespace Consumer.Core.Entities.Campaigns;

public sealed record EventPreparationState(
    Guid EventId,
    string EventType,
    Guid EventTypeVersionId,
    int EventVersion,
    string RoutingKey,
    DateTime OccurredAt,
    string PayloadHash,
    string Status,
    IReadOnlyList<PreparedCampaignTarget> Targets);
