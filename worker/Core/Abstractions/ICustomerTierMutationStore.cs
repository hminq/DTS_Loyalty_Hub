using Core.Entities;

namespace Core.Abstractions;

public interface ICustomerTierMutationStore
{
    Task ApplyBatchAsync(
        IReadOnlyList<CustomerTierExpirationMutation> mutations,
        DateTime processedAt,
        CancellationToken cancellationToken);
}
