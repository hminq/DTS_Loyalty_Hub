using Core.Abstractions;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class ProcessExpiredCustomerTiersCommandHandler
    : IRequestHandler<ProcessExpiredCustomerTiersCommand, int>
{
    private readonly ICustomerTierRepository _repository;
    private readonly ICustomerTierMutationStore _mutationStore;

    public ProcessExpiredCustomerTiersCommandHandler(
        ICustomerTierRepository repository,
        ICustomerTierMutationStore mutationStore)
    {
        _repository = repository;
        _mutationStore = mutationStore;
    }

    public async Task<int> Handle(
        ProcessExpiredCustomerTiersCommand request,
        CancellationToken cancellationToken)
    {
        var tiers = (await _repository.GetTierConfigurationsAsync(cancellationToken))
            .OrderBy(tier => tier.Priority)
            .ToArray();

        if (tiers.Length == 0)
        {
            return 0;
        }

        var tierIndexById = tiers
            .Select((tier, index) => (tier.TierConfigId, index))
            .ToDictionary(item => item.TierConfigId, item => item.index);

        var expiredCustomers = await _repository.GetExpiredCustomersAsync(
            request.ProcessedAt,
            cancellationToken);

        var processedCount = 0;
        foreach (var customer in expiredCustomers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!tierIndexById.TryGetValue(customer.TierConfigId, out var currentTierIndex))
            {
                continue;
            }

            var targetTierIndex = Math.Max(0, currentTierIndex - 1);
            var targetTier = tiers[targetTierIndex];
            var nextTier = targetTierIndex + 1 < tiers.Length
                ? tiers[targetTierIndex + 1]
                : null;

            if (currentTierIndex == 0)
            {
                await _mutationStore.ResetToMinTierAsync(
                    customer.CustomerId,
                    request.ProcessedAt,
                    request.ProcessedAt.AddMonths(targetTier.CycleMonth),
                    nextTier?.TierConfigId,
                    nextTier?.PointsRequired ?? targetTier.PointsRequired,
                    customer.CurrentTierPoint,
                    cancellationToken);
            }
            else
            {
                await _mutationStore.DowngradeTierAsync(
                    customer.CustomerId,
                    targetTier.TierConfigId,
                    targetTier.PointsRequired,
                    request.ProcessedAt,
                    request.ProcessedAt.AddMonths(targetTier.CycleMonth),
                    nextTier?.TierConfigId,
                    nextTier?.PointsRequired ?? targetTier.PointsRequired,
                    customer.CurrentTierPoint,
                    targetTier.PointsRequired,
                    cancellationToken);
            }

            processedCount++;
        }

        return processedCount;
    }
}
