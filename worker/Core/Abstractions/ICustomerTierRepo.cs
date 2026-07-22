
namespace Core.Abstractions;

public interface ICustomerTierRepo
{

    Task<int> CheckAndProcessExpiredTiersAsync(CancellationToken cancellationToken = default);
}