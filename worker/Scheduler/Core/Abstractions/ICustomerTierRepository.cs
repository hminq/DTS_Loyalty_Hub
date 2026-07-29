using Scheduler.Core.Entities;

namespace Scheduler.Core.Abstractions;

public interface ICustomerTierRepository
{
    Task<IReadOnlyList<TierConfiguration>> GetTierConfigurationsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ExpiredCustomerTier>> GetExpiredCustomersAsync(
        DateTime expiresAtOrBefore,
        int batchSize,
        CancellationToken cancellationToken);
}
