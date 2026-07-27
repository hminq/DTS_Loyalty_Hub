using MediatR;

namespace Core.Requests;

public sealed record GetDispatchableOutboxMessageIdsQuery(DateTime Now)
    : IRequest<IReadOnlyList<Guid>>;
