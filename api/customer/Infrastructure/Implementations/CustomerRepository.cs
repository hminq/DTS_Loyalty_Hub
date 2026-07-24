using Core.Abstractions;
using Core.UseCases.Customers.Queries.GetProfileAndWallet;
using Persistence.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public CustomerRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProfileAndWalletResult?> GetProfileAndWalletAsync(
        Guid customerId,
        CancellationToken ct)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.CustomerPoint)
            .Include(c => c.Tier)
            .Where(c => c.CustomerId == customerId)
            .FirstOrDefaultAsync(ct);

        if (customer is null)
        {
            return null;
        }

        var profile = new UserProfileResult(
            customer.User.Username,
            customer.User.Email,
            customer.User.FullName,
            customer.User.PhoneNumber,
            customer.Tier?.Name,
            customer.CurrentTierPoint,
            customer.NextTierPoint
        );

        var wallet = new UserWalletResult(
            customer.CustomerPoint?.ActivePoint ?? 0,
            customer.CustomerPoint?.LockedPoint ?? 0,
            customer.CustomerPoint?.LifetimePoint ?? 0,
            customer.CustomerPoint?.SpentPoint ?? 0,
            customer.CustomerPoint?.ExpiredPoint ?? 0
        );

        return new ProfileAndWalletResult(profile, wallet);
    }

    public async Task<CustomerWithTiersResult?> GetCustomerWithTiersAsync(
        Guid customerId,
        CancellationToken ct)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Tier)
            .Include(c => c.NextTier)
            .Where(c => c.CustomerId == customerId)
            .FirstOrDefaultAsync(ct);

        if (customer is null)
        {
            return null;
        }

        return new CustomerWithTiersResult(
            FullName: customer.User.FullName,
            CurrentTierName: customer.Tier?.Name,
            CurrentTierPoint: customer.CurrentTierPoint,
            NextTierName: customer.NextTier?.Name,
            NextTierPoint: customer.NextTierPoint);
    }
}
