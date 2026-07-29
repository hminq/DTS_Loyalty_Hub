namespace Messaging.Contracts.Events;

public sealed record EventEnvelope<TPayload>(
    Guid EventId,
    string EventType,
    int EventVersion,
    DateTimeOffset OccurredAt,
    TPayload Payload);
