using System.Text.Json;

namespace Consumer.Core.Entities.Envelope;

public sealed record RawEventEnvelope(
    Guid EventId,
    string EventType,
    int EventVersion,
    DateTime OccurredAt,
    JsonElement PayloadElement);
