namespace Core.Entities;

public sealed record OutgoingMessage(
    Guid EventId,
    string EventType,
    string RoutingKey,
    string Body);
