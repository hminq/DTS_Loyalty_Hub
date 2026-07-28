using System.Text.Json;
using Core.Abstractions;
using Messaging.Contracts.Events;
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

    public void Add<TData>(OutgoingEvent<TData> outgoingEvent)
    {
        var payload = JsonSerializer.Serialize(
            outgoingEvent,
            SerializerOptions);

        _dbContext.OutboxMessages.Add(
            new OutboxMessage
            {
                EventId = outgoingEvent.EventId,
                EventType = outgoingEvent.EventType,
                RoutingKey = outgoingEvent.RoutingKey,
                Payload = payload,
                Status = OutboxMessageStatuses.Pending,
                AttemptCount = 0,
                NextAttemptAt = outgoingEvent.OccurredAt,
                LastErrorCode = null,
                LastError = null,
                OccurredAt = outgoingEvent.OccurredAt,
                CreatedAt = outgoingEvent.OccurredAt,
                PublishedAt = null
            });
    }
}
