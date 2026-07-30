using Scheduler.Core.Entities;

namespace Scheduler.Core.Abstractions;

public interface ICustomerTierMutationStore
{
    Task ApplyBatchAsync(
        IReadOnlyList<CustomerTierExpirationMutation> mutations,
        DateTime processedAt,
        CancellationToken cancellationToken);
}
