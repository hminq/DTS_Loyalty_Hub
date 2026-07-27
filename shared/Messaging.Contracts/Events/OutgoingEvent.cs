namespace Messaging.Contracts.Events;

public sealed record OutgoingEvent<TData>(
    Guid EventId,
    string EventType,
    string RoutingKey,
    DateTime OccurredAt,
    TData Data);
