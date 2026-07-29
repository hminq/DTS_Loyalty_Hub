using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Consumer.Infrastructure.Implementations;

public sealed class IssuePointExecutionStore : IIssuePointExecutionStore
{
    private const decimal MaximumNumeric18Scale2 = 9999999999999999.99m;

    private readonly LoyaltyHubDbContext _dbContext;

    public IssuePointExecutionStore(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task LockCustomerPointsAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default)
    {
        if (customerIds.Count == 0)
        {
            return;
        }

        var orderedIds = customerIds.Distinct().Order().ToArray();
        await _dbContext.CustomerPoints
            .FromSqlInterpolated($$"""
                SELECT *
                FROM customer_points
                WHERE customer_id = ANY ({{orderedIds}})
                ORDER BY customer_id
                FOR UPDATE
                """)
            .LoadAsync(cancellationToken);
    }

    public void ApplyIssuePoint(IssuePointMutation mutation)
    {
        var customerPoint = _dbContext.CustomerPoints.Local.SingleOrDefault(
            item => item.CustomerId == mutation.CustomerId);

        if (customerPoint is null)
        {
            customerPoint = new CustomerPoint
            {
                CustomerPointId = mutation.CustomerPointId,
                CustomerId = mutation.CustomerId,
                ActivePoint = 0,
                LockedPoint = 0,
                LifetimePoint = 0,
                SpentPoint = 0,
                ExpiredPoint = 0,
                CreatedAt = mutation.CreatedAt,
                UpdatedAt = mutation.CreatedAt
            };
            _dbContext.CustomerPoints.Add(customerPoint);
        }

        decimal balanceAfter;
        decimal lifetimeAfter;
        try
        {
            balanceAfter = checked(customerPoint.ActivePoint + mutation.Amount);
            lifetimeAfter = checked(customerPoint.LifetimePoint + mutation.Amount);
        }
        catch (OverflowException)
        {
            throw PersistenceFailure();
        }

        if (balanceAfter > MaximumNumeric18Scale2 ||
            lifetimeAfter > MaximumNumeric18Scale2)
        {
            throw PersistenceFailure();
        }

        var balanceBefore = customerPoint.ActivePoint;
        customerPoint.ActivePoint = balanceAfter;
        customerPoint.LifetimePoint = lifetimeAfter;
        customerPoint.UpdatedAt = mutation.CreatedAt;

        _dbContext.PointTransactions.Add(
            new PointTransaction
            {
                PointTransactionId = mutation.PointTransactionId,
                CustomerId = mutation.CustomerId,
                CampaignId = mutation.CampaignId,
                CampaignSessionId = mutation.CampaignSessionId,
                ActionId = mutation.ActionId,
                SourceEventId = mutation.EventId.ToString("D"),
                TransactionType = PointTransactionTypes.CampaignReward,
                Amount = mutation.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                CreatedAt = mutation.CreatedAt
            });
    }

    private static CampaignProcessingException PersistenceFailure()
    {
        return new CampaignProcessingException(
            CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
            retriable: true);
    }
}
