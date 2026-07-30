namespace Scheduler.Core.Entities;

public sealed record OutboxDispatchMessage(
    Guid EventId,
    Guid EventTypeVersionId,
    string EventType,
    int EventVersion,
    string RoutingKey,
    string Payload,
    int AttemptCount);
