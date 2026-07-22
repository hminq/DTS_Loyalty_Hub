using Core.Abstractions;
using Core.Entities;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class ProcessExpiredCustomerTierBatchCommandHandler
    : IRequestHandler<ProcessExpiredCustomerTierBatchCommand, ProcessExpiredCustomerTierBatchResult>
{
    private readonly ICustomerTierRepository _repository;
    private readonly ICustomerTierMutationStore _mutationStore;

    public ProcessExpiredCustomerTierBatchCommandHandler(
        ICustomerTierRepository repository,
        ICustomerTierMutationStore mutationStore)
    {
        _repository = repository;
        _mutationStore = mutationStore;
    }

    public async Task<ProcessExpiredCustomerTierBatchResult> Handle(
        ProcessExpiredCustomerTierBatchCommand request,
        CancellationToken cancellationToken)
    {
        if (request.BatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.BatchSize),
                request.BatchSize,
                "Batch size must be greater than zero.");
        }

        var tiers = (await _repository.GetTierConfigurationsAsync(cancellationToken))
            .OrderBy(tier => tier.Priority)
            .ToArray();

        if (tiers.Length == 0)
        {
            return new ProcessExpiredCustomerTierBatchResult(0, 0);
        }

        var tierIndexById = tiers
            .Select((tier, index) => (tier.TierConfigId, index))
            .ToDictionary(item => item.TierConfigId, item => item.index);

        var expiredCustomers = await _repository.GetExpiredCustomersAsync(
            request.ProcessedAt,
            request.BatchSize,
            cancellationToken);

        var mutations = new List<CustomerTierExpirationMutation>(expiredCustomers.Count);
        foreach (var customer in expiredCustomers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!tierIndexById.TryGetValue(customer.TierConfigId, out var currentTierIndex))
            {
                throw new InvalidOperationException(
                    $"Customer '{customer.CustomerId}' references missing tier configuration '{customer.TierConfigId}'.");
            }

            var targetTierIndex = Math.Max(0, currentTierIndex - 1);
            var targetTier = tiers[targetTierIndex];
            var nextTier = targetTierIndex + 1 < tiers.Length
                ? tiers[targetTierIndex + 1]
                : null;

            mutations.Add(new CustomerTierExpirationMutation(
                customer.CustomerId,
                targetTier.TierConfigId,
                currentTierIndex == 0 ? 0 : targetTier.PointsRequired,
                request.ProcessedAt,
                request.ProcessedAt.AddMonths(targetTier.CycleMonth),
                nextTier?.TierConfigId,
                nextTier?.PointsRequired ?? targetTier.PointsRequired,
                customer.CurrentTierPoint));
        }

        await _mutationStore.ApplyBatchAsync(
            mutations,
            request.ProcessedAt,
            cancellationToken);

        return new ProcessExpiredCustomerTierBatchResult(
            expiredCustomers.Count,
            mutations.Count);
    }
}
