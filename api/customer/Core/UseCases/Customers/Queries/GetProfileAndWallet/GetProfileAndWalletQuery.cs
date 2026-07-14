using MediatR;

namespace Core.UseCases.Customers.Queries.GetProfileAndWallet;

public sealed record GetProfileAndWalletQuery(Guid CustomerId) : IRequest<ProfileAndWalletResult?>;
