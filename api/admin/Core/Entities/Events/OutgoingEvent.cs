namespace Core.Entities.Events;

public sealed record OutgoingEvent
{
    public OutgoingEvent(
        Guid eventId,
        string eventType,
        string routingKey,
        string body)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID must not be empty.", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type must not be blank.", nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(routingKey))
        {
            throw new ArgumentException("Routing key must not be blank.", nameof(routingKey));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Event body must not be blank.", nameof(body));
        }

        EventId = eventId;
        EventType = eventType;
        RoutingKey = routingKey;
        Body = body;
    }

    public Guid EventId { get; }

    public string EventType { get; }

    public string RoutingKey { get; }

    public string Body { get; }
}
