using Core.Abstractions;
using Core.Entities.Constants;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

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
