using Core.Abstractions;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class CustomerTierRepository : ICustomerTierRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public CustomerTierRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TierConfiguration>> GetTierConfigurationsAsync(
        CancellationToken cancellationToken)
        => await _dbContext.TiersConfigs
            .AsNoTracking()
            .Select(tier => new TierConfiguration(
                tier.TierConfigId,
                tier.PointsRequired,
                tier.CycleMonth,
                tier.Priority))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ExpiredCustomerTier>> GetExpiredCustomersAsync(
        DateTime expiresAtOrBefore,
        int batchSize,
        CancellationToken cancellationToken)
    {
        // The transaction behavior has already opened the transaction. Lock each
        // selected customer so another Scheduler instance skips it instead of
        // producing the same reset transactions concurrently.
        var customers = await _dbContext.Customers
            .FromSqlInterpolated($$"""
                SELECT *
                FROM customer
                WHERE expired_tier IS NOT NULL
                  AND expired_tier <= {{expiresAtOrBefore}}
                  AND tier_id IS NOT NULL
                ORDER BY expired_tier, customer_id
                LIMIT {{batchSize}}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        return customers
            .Select(customer => new ExpiredCustomerTier(
                customer.CustomerId,
                customer.TierId!.Value,
                customer.CurrentTierPoint))
            .ToArray();
    }
}
