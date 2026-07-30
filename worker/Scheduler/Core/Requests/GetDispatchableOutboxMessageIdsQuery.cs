using MediatR;

namespace Scheduler.Core.Requests;

public sealed record GetDispatchableOutboxMessageIdsQuery(DateTime Now)
    : IRequest<IReadOnlyList<Guid>>;
