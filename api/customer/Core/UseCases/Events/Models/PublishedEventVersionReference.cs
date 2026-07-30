namespace Core.UseCases.Events.Models;

public sealed record PublishedEventVersionReference(
    Guid EventTypeVersionId,
    string EventType,
    int EventVersion,
    string RoutingKey);
