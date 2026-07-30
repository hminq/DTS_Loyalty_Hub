using System.Text.Json;
using Core.Abstractions;
using Core.UseCases.Events.Models;
using Messaging.Contracts.Outbox;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class OutboxWriter : IOutboxWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly LoyaltyHubDbContext _dbContext;

    public OutboxWriter(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add<TPayload>(VersionedOutboxEvent<TPayload> outboxEvent)
    {
        if (!string.Equals(
                outboxEvent.PublishedVersion.EventType,
                outboxEvent.Envelope.EventType,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Outbox event type does not match the published event reference.");
        }

        if (outboxEvent.PublishedVersion.EventVersion != outboxEvent.Envelope.EventVersion)
        {
            throw new InvalidOperationException("Outbox event version does not match the published event reference.");
        }

        var payload = JsonSerializer.Serialize(
            outboxEvent.Envelope,
            SerializerOptions);

        _dbContext.OutboxMessages.Add(
            new OutboxMessage
            {
                EventId = outboxEvent.Envelope.EventId,
                EventType = outboxEvent.PublishedVersion.EventType,
                RoutingKey = outboxEvent.PublishedVersion.RoutingKey,
                EventTypeVersionId = outboxEvent.PublishedVersion.EventTypeVersionId,
                EventVersion = outboxEvent.PublishedVersion.EventVersion,
                Payload = payload,
                Status = OutboxMessageStatuses.Pending,
                AttemptCount = 0,
                NextAttemptAt = outboxEvent.Envelope.OccurredAt,
                LastErrorCode = null,
                LastError = null,
                OccurredAt = outboxEvent.Envelope.OccurredAt,
                CreatedAt = outboxEvent.Envelope.OccurredAt,
                PublishedAt = null
            });
    }
}
