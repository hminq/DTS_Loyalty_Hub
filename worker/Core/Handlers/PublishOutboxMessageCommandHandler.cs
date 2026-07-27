using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using MediatR;
using Messaging.Contracts.Outbox;

namespace Core.Handlers;

public sealed class PublishOutboxMessageCommandHandler
    : IRequestHandler<PublishOutboxMessageCommand, PublishOutboxMessageResult>
{
    private const int MaximumStoredErrorLength = 1000;

    private readonly IOutboxDispatchStore _store;
    private readonly IEventPublisher _publisher;
    private readonly TimeProvider _timeProvider;

    public PublishOutboxMessageCommandHandler(
        IOutboxDispatchStore store,
        IEventPublisher publisher,
        TimeProvider timeProvider)
    {
        _store = store;
        _publisher = publisher;
        _timeProvider = timeProvider;
    }

    public async Task<PublishOutboxMessageResult> Handle(
        PublishOutboxMessageCommand request,
        CancellationToken cancellationToken)
    {
        var attemptedAt = _timeProvider.GetUtcNow().UtcDateTime;
        var message = await _store.GetDispatchableAsync(
            request.EventId,
            attemptedAt,
            OutboxDispatchConstants.MaxAttempts,
            cancellationToken);

        if (message is null)
        {
            return new PublishOutboxMessageResult(
                request.EventId,
                OutboxDispatchOutcome.Skipped);
        }

        var attemptCount = message.AttemptCount + 1;

        try
        {
            await _publisher.PublishAsync(
                new OutgoingMessage(
                    message.EventId,
                    message.EventType,
                    message.RoutingKey,
                    message.Payload),
                cancellationToken);

            _store.MarkPublished(message.EventId, attemptCount, attemptedAt);

            return new PublishOutboxMessageResult(
                message.EventId,
                OutboxDispatchOutcome.Published);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OutboxPublishException exception)
        {
            return RecordFailure(message.EventId, attemptCount, attemptedAt, exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            return RecordFailure(
                message.EventId,
                attemptCount,
                attemptedAt,
                OutboxDispatchErrorCodes.UnexpectedError,
                exception.Message);
        }
    }

    private PublishOutboxMessageResult RecordFailure(
        Guid eventId,
        int attemptCount,
        DateTime attemptedAt,
        string errorCode,
        string error)
    {
        var exhausted = attemptCount >= OutboxDispatchConstants.MaxAttempts;
        var nextAttemptAt = exhausted
            ? attemptedAt
            : attemptedAt.AddSeconds(
                OutboxDispatchConstants.BaseRetrySeconds
                * Math.Pow(2, attemptCount - 1));

        _store.RecordFailure(
            eventId,
            attemptCount,
            exhausted ? OutboxMessageStatuses.Failed : OutboxMessageStatuses.Pending,
            nextAttemptAt,
            errorCode,
            Sanitize(error));

        return new PublishOutboxMessageResult(
            eventId,
            exhausted ? OutboxDispatchOutcome.Failed : OutboxDispatchOutcome.Retried);
    }

    private static string Sanitize(string error)
    {
        var normalized = string.Join(
            ' ',
            error.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return normalized.Length <= MaximumStoredErrorLength
            ? normalized
            : normalized[..MaximumStoredErrorLength];
    }
}
