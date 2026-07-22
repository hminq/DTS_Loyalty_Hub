using Core.Abstractions;
using Core.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

public sealed class CustomerTierRepository : ICustomerTierRepo
{
    private readonly LoyaltyHubDbContext _dbContext;
    private readonly IMediator _mediator;

    public CustomerTierRepository(LoyaltyHubDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<int> CheckAndProcessExpiredTiersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var tierConfigs = await _dbContext.TiersConfigs
            .AsNoTracking()
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);

        if (tierConfigs.Count == 0)
        {
            return 0;
        }

        var configById = tierConfigs.ToDictionary(t => t.TierConfigId);

        var lowerOf = new Dictionary<Guid, Persistence.Models.TiersConfig?>();
        var higherOf = new Dictionary<Guid, Persistence.Models.TiersConfig?>();
        for (int i = 0; i < tierConfigs.Count; i++)
        {
            lowerOf[tierConfigs[i].TierConfigId] = i > 0 ? tierConfigs[i - 1] : null;
            higherOf[tierConfigs[i].TierConfigId] = i < tierConfigs.Count - 1 ? tierConfigs[i + 1] : null;
        }

        var expiredCustomers = await _dbContext.Customers
            .Where(c => c.ExpiredTier != null && c.ExpiredTier <= now && c.TierId != null)
            .ToListAsync(cancellationToken);

        int updatedCount = 0;

        foreach (var customer in expiredCustomers)
        {
            if (customer.TierId is null || !configById.TryGetValue(customer.TierId.Value, out var currentConfig))
            {
                continue;
            }

            var tierPointBefore = customer.CurrentTierPoint;
            var lowerConfig = lowerOf[currentConfig.TierConfigId];

            if (lowerConfig is null)
            {
                await _mediator.Send(
                    new ResetCustomerToMinTierCommand(
                        CustomerId: customer.CustomerId,
                        StartTier: now,
                        ExpiredTier: now.AddMonths(currentConfig.CycleMonth),
                        TierPointBefore: tierPointBefore),
                    cancellationToken);
            }
            else
            {
                var nextConfig = higherOf[lowerConfig.TierConfigId];

                await _mediator.Send(
                    new DowngradeCustomerTierCommand(
                        CustomerId: customer.CustomerId,
                        NewTierConfigId: lowerConfig.TierConfigId,
                        NewCurrentTierPoint: lowerConfig.PointsRequired,
                        StartTier: now,
                        ExpiredTier: now.AddMonths(lowerConfig.CycleMonth),
                        NextTierConfigId: nextConfig?.TierConfigId,
                        NextTierPoint: nextConfig?.PointsRequired ?? lowerConfig.PointsRequired,
                        TierPointBefore: tierPointBefore,
                        TierPointAfter: lowerConfig.PointsRequired),
                    cancellationToken);
            }

            updatedCount++;
        }

        return updatedCount;
    }
}