
using MediatR;
using Core.Abstractions;

namespace Core.Requests;

public sealed record DowngradeCustomerTierCommand(
    Guid CustomerId,
    Guid NewTierConfigId,
    decimal NewCurrentTierPoint,
    DateTime StartTier,
    DateTime ExpiredTier,
    Guid? NextTierConfigId,
    decimal NextTierPoint,
    decimal TierPointBefore,
    decimal TierPointAfter
) : IRequest, ITransactionalRequest;