using Core.Abstractions;
using MediatR;

namespace Core.Requests;

public sealed record ResetCustomerToMinTierCommand(
    Guid CustomerId,
    DateTime StartTier,
    DateTime ExpiredTier,
    decimal TierPointBefore
) : IRequest, ITransactionalRequest;