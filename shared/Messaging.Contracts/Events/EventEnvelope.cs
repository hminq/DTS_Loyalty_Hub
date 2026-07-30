namespace Messaging.Contracts.Events;

public sealed record EventEnvelope<TPayload>
{
    public EventEnvelope(
        Guid eventId,
        string eventType,
        int eventVersion,
        DateTime occurredAt,
        TPayload payload)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID must not be empty.", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type must not be blank.", nameof(eventType));
        }

        if (eventVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(eventVersion), eventVersion, "Event version must be positive.");
        }

        if (occurredAt.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("OccurredAt must be UTC.", nameof(occurredAt));
        }

        ArgumentNullException.ThrowIfNull(payload);

        EventId = eventId;
        EventType = eventType;
        EventVersion = eventVersion;
        OccurredAt = occurredAt;
        Payload = payload;
    }

    public Guid EventId { get; init; }

    public string EventType { get; init; }

    public int EventVersion { get; init; }

    public DateTime OccurredAt { get; init; }

    public TPayload Payload { get; init; }
}
