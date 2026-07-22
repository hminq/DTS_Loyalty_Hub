using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class CustomerTierMutationStore : ICustomerTierMutationStore
{
    private readonly LoyaltyHubDbContext _dbContext;

    public CustomerTierMutationStore(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ResetToMinTierAsync(
        Guid customerId,
        DateTime startTier,
        DateTime expiredTier,
        Guid? nextTierConfigId,
        decimal nextTierPoint,
        decimal tierPointBefore,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var customer = await _dbContext.Customers
            .FirstAsync(c => c.CustomerId == customerId, cancellationToken);

        customer.CurrentTierPoint = 0;
        customer.StartTier = startTier;
        customer.ExpiredTier = expiredTier;
        customer.NextTierId = nextTierConfigId;
        customer.NextTierPoint = nextTierPoint;

        // Đã ở hạng sàn nhưng vẫn hết chu kỳ -> vẫn mất active point + ghi log, y như downgrade.
        await ApplyPointLossAsync(customerId, tierPointBefore, tierPointAfter: 0, now, cancellationToken);
    }

    public async Task DowngradeTierAsync(
        Guid customerId,
        Guid newTierConfigId,
        decimal newCurrentTierPoint,
        DateTime startTier,
        DateTime expiredTier,
        Guid? nextTierConfigId,
        decimal nextTierPoint,
        decimal tierPointBefore,
        decimal tierPointAfter,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var customer = await _dbContext.Customers
            .FirstAsync(c => c.CustomerId == customerId, cancellationToken);

        customer.TierId = newTierConfigId;
        customer.CurrentTierPoint = newCurrentTierPoint;
        customer.StartTier = startTier;
        customer.ExpiredTier = expiredTier;
        customer.NextTierId = nextTierConfigId;
        customer.NextTierPoint = nextTierPoint;

        await ApplyPointLossAsync(customerId, tierPointBefore, tierPointAfter, now, cancellationToken);
    }

    private async Task ApplyPointLossAsync(
        Guid customerId,
        decimal tierPointBefore,
        decimal tierPointAfter,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var customerPoint = await _dbContext.CustomerPoints
            .FirstOrDefaultAsync(cp => cp.CustomerId == customerId, cancellationToken);

        if (customerPoint is null)
        {
            return;
        }

        var lostActivePoint = customerPoint.ActivePoint;

        customerPoint.ExpiredPoint += lostActivePoint;
        customerPoint.ActivePoint = 0;
        customerPoint.UpdatedAt = now;

        if (lostActivePoint != 0)
        {
            _dbContext.PointTransactions.Add(new PointTransaction
            {
                PointTransactionId = Guid.NewGuid(),
                CustomerId = customerId,
                CampaignId = null,
                CampaignSessionId = null,
                ActionId = null,
                SourceEventId = null,
                TransactionType = PointTransactionTypes.ActivePointReset,
                Amount = PointResetAmounts.Calculate(lostActivePoint, 0),
                BalanceBefore = lostActivePoint,
                BalanceAfter = 0,
                CreatedAt = now,
            });
        }

        var tierPointChange = PointResetAmounts.Calculate(tierPointBefore, tierPointAfter);
        if (tierPointChange != 0)
        {
            _dbContext.PointTransactions.Add(new PointTransaction
            {
                PointTransactionId = Guid.NewGuid(),
                CustomerId = customerId,
                CampaignId = null,
                CampaignSessionId = null,
                ActionId = null,
                SourceEventId = null,
                TransactionType = PointTransactionTypes.TierPointReset,
                Amount = tierPointChange,
                BalanceBefore = tierPointBefore,
                BalanceAfter = tierPointAfter,
                CreatedAt = now,
            });
        }
    }
}
