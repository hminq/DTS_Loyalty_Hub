using Scheduler.Core.Abstractions;
using Scheduler.Core.Entities.Constants;
using Scheduler.Core.Requests;
using MediatR;

namespace Scheduler.Core.Handlers;

public sealed class GetDispatchableOutboxMessageIdsQueryHandler
    : IRequestHandler<GetDispatchableOutboxMessageIdsQuery, IReadOnlyList<Guid>>
{
    private readonly IOutboxDispatchStore _store;

    public GetDispatchableOutboxMessageIdsQueryHandler(IOutboxDispatchStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Guid>> Handle(
        GetDispatchableOutboxMessageIdsQuery request,
        CancellationToken cancellationToken)
    {
        return _store.GetDispatchableIdsAsync(
            request.Now,
            OutboxDispatchConstants.BatchSize,
            OutboxDispatchConstants.MaxAttempts,
            cancellationToken);
    }
}
