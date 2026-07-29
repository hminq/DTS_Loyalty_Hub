using Scheduler.Core.Abstractions;
using MediatR;

namespace Scheduler.Core.Requests;

public sealed record PublishOutboxMessageCommand(Guid EventId)
    : IRequest<PublishOutboxMessageResult>, IWriteRequest;

public sealed record PublishOutboxMessageResult(
    Guid EventId,
    OutboxDispatchOutcome Outcome);

public enum OutboxDispatchOutcome
{
    Published,
    Retried,
    Failed,
    Skipped
}
