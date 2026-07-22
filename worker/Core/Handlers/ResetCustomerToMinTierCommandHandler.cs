using Core.Abstractions;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class ResetCustomerToMinTierCommandHandler : IRequestHandler<ResetCustomerToMinTierCommand>
{
    private readonly ICustomerTierMutationStore _store;

    public ResetCustomerToMinTierCommandHandler(ICustomerTierMutationStore store)
    {
        _store = store;
    }

    public Task Handle(ResetCustomerToMinTierCommand request, CancellationToken cancellationToken)
        => _store.ResetToMinTierAsync(
            request.CustomerId,
            request.StartTier,
            request.ExpiredTier,
            request.TierPointBefore,
            cancellationToken);
}