namespace Core.Entities;

public sealed record OutboxDispatchMessage(
    Guid EventId,
    string EventType,
    string RoutingKey,
    string Payload,
    int AttemptCount);
