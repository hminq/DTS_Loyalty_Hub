using Scheduler.Core.Abstractions;
using Scheduler.Core.Entities;
using Messaging.Contracts.Outbox;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Scheduler.Infrastructure.Implementations;

public sealed class OutboxDispatchStore : IOutboxDispatchStore
{
    private readonly LoyaltyHubDbContext _dbContext;

    public OutboxDispatchStore(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Guid>> GetDispatchableIdsAsync(
        DateTime now,
        int batchSize,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxMessages
            .AsNoTracking()
            .Where(message =>
                message.Status == OutboxMessageStatuses.Pending
                && message.NextAttemptAt <= now
                && message.AttemptCount < maxAttempts)
            .OrderBy(message => message.NextAttemptAt)
            .ThenBy(message => message.CreatedAt)
            .Select(message => message.EventId)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<OutboxDispatchMessage?> GetDispatchableAsync(
        Guid eventId,
        DateTime now,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.OutboxMessages
            .Where(message =>
                message.EventId == eventId
                && message.Status == OutboxMessageStatuses.Pending
                && message.NextAttemptAt <= now
                && message.AttemptCount < maxAttempts)
            .SingleOrDefaultAsync(cancellationToken);

        return entity is null
            ? null
            : new OutboxDispatchMessage(
                entity.EventId,
                entity.EventType,
                entity.RoutingKey,
                entity.Payload,
                entity.AttemptCount);
    }

    public void MarkPublished(Guid eventId, int attemptCount, DateTime publishedAt)
    {
        var entity = GetTracked(eventId);
        entity.Status = OutboxMessageStatuses.Published;
        entity.AttemptCount = attemptCount;
        entity.PublishedAt = publishedAt;
        entity.LastErrorCode = null;
        entity.LastError = null;
    }

    public void RecordFailure(
        Guid eventId,
        int attemptCount,
        string status,
        DateTime nextAttemptAt,
        string errorCode,
        string error)
    {
        var entity = GetTracked(eventId);
        entity.Status = status;
        entity.AttemptCount = attemptCount;
        entity.NextAttemptAt = nextAttemptAt;
        entity.LastErrorCode = errorCode;
        entity.LastError = error;
        entity.PublishedAt = null;
    }

    private OutboxMessage GetTracked(Guid eventId)
    {
        return _dbContext.OutboxMessages.Local.Single(message => message.EventId == eventId);
    }
}
