namespace Core.Entities.Campaigns;

public sealed record CampaignEventDelivery(
    ReadOnlyMemory<byte> Body,
    string? MessageId,
    string? MessageType,
    string RoutingKey,
    bool Redelivered);
