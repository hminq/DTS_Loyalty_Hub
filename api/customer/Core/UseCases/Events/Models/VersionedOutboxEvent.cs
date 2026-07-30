using Messaging.Contracts.Events;

namespace Core.UseCases.Events.Models;

public sealed record VersionedOutboxEvent<TPayload>(
    PublishedEventVersionReference PublishedVersion,
    EventEnvelope<TPayload> Envelope);
