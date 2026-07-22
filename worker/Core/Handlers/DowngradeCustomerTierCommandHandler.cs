using Core.Abstractions;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class DowngradeCustomerTierCommandHandler : IRequestHandler<DowngradeCustomerTierCommand>
{
    private readonly ICustomerTierMutationStore _store;

    public DowngradeCustomerTierCommandHandler(ICustomerTierMutationStore store)
    {
        _store = store;
    }

    public Task Handle(DowngradeCustomerTierCommand request, CancellationToken cancellationToken)
        => _store.DowngradeTierAsync(
            request.CustomerId,
            request.NewTierConfigId,
            request.NewCurrentTierPoint,
            request.StartTier,
            request.ExpiredTier,
            request.NextTierConfigId,
            request.NextTierPoint,
            request.TierPointBefore,
            request.TierPointAfter,
            cancellationToken);
}