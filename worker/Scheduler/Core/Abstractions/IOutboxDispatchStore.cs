using Scheduler.Core.Entities;

namespace Scheduler.Core.Abstractions;

public interface IOutboxDispatchStore
{
    Task<IReadOnlyList<Guid>> GetDispatchableIdsAsync(
        DateTime now,
        int batchSize,
        int maxAttempts,
        CancellationToken cancellationToken);

    Task<OutboxDispatchMessage?> GetDispatchableAsync(
        Guid eventId,
        DateTime now,
        int maxAttempts,
        CancellationToken cancellationToken);

    void MarkPublished(Guid eventId, int attemptCount, DateTime publishedAt);

    void RecordFailure(
        Guid eventId,
        int attemptCount,
        string status,
        DateTime nextAttemptAt,
        string errorCode,
        string error);
}
