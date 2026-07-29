namespace Messaging.Contracts.Events;

public sealed record EventPayloadSchema(
    IReadOnlyList<EventPayloadFieldSchema> Fields,
    IReadOnlyList<EventTargetSchema> Targets);

public sealed record EventPayloadFieldSchema(
    string Code,
    string Type,
    string? Format,
    bool Required,
    bool Conditionable);

public sealed record EventTargetSchema(
    string Selector,
    string Kind,
    string IdField);
