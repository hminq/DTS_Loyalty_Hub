using Core.Abstractions;
using MediatR;

namespace Core.UseCases.Customers.Queries.GetProfileAndWallet;

public sealed class GetProfileAndWalletQueryHandler : IRequestHandler<GetProfileAndWalletQuery, ProfileAndWalletResult?>
{
    private readonly ICustomerRepository _customerRepository;

    public GetProfileAndWalletQueryHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<ProfileAndWalletResult?> Handle(GetProfileAndWalletQuery request,
CancellationToken ct)
    {
        return await _customerRepository.GetProfileAndWalletAsync(request.CustomerId, ct);
    }
}
