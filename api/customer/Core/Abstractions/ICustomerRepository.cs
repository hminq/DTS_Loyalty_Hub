using Core.UseCases.Customers.Queries.GetProfileAndWallet;

namespace Core.Abstractions;

public interface ICustomerRepository
{
    Task<ProfileAndWalletResult?> GetProfileAndWalletAsync(Guid customerId, CancellationToken ct);
    Task<CustomerWithTiersResult?> GetCustomerWithTiersAsync(Guid customerId, CancellationToken ct);
}
