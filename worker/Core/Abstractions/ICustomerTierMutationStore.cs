namespace Core.Abstractions;

public interface ICustomerTierMutationStore
{
    Task ResetToMinTierAsync(
        Guid customerId,
        DateTime startTier,
        DateTime expiredTier,
        decimal tierPointBefore,
        CancellationToken cancellationToken);

    Task DowngradeTierAsync(
        Guid customerId,
        Guid newTierConfigId,
        decimal newCurrentTierPoint,
        DateTime startTier,
        DateTime expiredTier,
        Guid? nextTierConfigId,
        decimal nextTierPoint,
        decimal tierPointBefore,
        decimal tierPointAfter,
        CancellationToken cancellationToken);
}