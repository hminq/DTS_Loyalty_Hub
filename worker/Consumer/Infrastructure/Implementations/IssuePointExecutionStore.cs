using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Entities.Points;
using Consumer.Core.Exceptions;
using Consumer.Core.Services;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Consumer.Infrastructure.Implementations;

public sealed class IssuePointExecutionStore : IIssuePointExecutionStore
{
    private readonly LoyaltyHubDbContext _dbContext;
    private readonly CustomerTierProgressionService _tierProgressionService;
    private IReadOnlyList<TierProgressionConfiguration> _tierConfigurations = [];

    public IssuePointExecutionStore(
        LoyaltyHubDbContext dbContext,
        CustomerTierProgressionService tierProgressionService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tierProgressionService = tierProgressionService ??
            throw new ArgumentNullException(nameof(tierProgressionService));
    }

    public async Task PrepareIssuePointExecutionAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default)
    {
        _tierConfigurations = await _dbContext.TiersConfigs
            .AsNoTracking()
            .OrderBy(tier => tier.Priority)
            .Select(tier => new TierProgressionConfiguration(
                tier.TierConfigId,
                tier.PointsRequired,
                tier.CycleMonth,
                tier.Priority))
            .ToArrayAsync(cancellationToken);

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
        var customer = _dbContext.Customers.Local.SingleOrDefault(
            item => item.CustomerId == mutation.CustomerId)
            ?? throw PersistenceFailure();
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

        var tierMutation = _tierProgressionService.Calculate(
            new CustomerPointTierState(
                customer.CustomerId,
                customer.TierId,
                customer.CurrentTierPoint,
                customer.NextTierPoint,
                customer.StartTier,
                customer.ExpiredTier,
                customer.NextTierId,
                customerPoint.ActivePoint,
                customerPoint.LifetimePoint),
            _tierConfigurations,
            mutation.Amount,
            mutation.CreatedAt);

        customer.TierId = tierMutation.TierId;
        customer.CurrentTierPoint = tierMutation.CurrentTierPoint;
        customer.NextTierId = tierMutation.NextTierId;
        customer.NextTierPoint = tierMutation.NextTierPoint;
        customer.StartTier = tierMutation.StartTier;
        customer.ExpiredTier = tierMutation.ExpiredTier;
        customerPoint.ActivePoint = tierMutation.ActivePointAfter;
        customerPoint.LifetimePoint = tierMutation.LifetimePointAfter;
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
                BalanceBefore = tierMutation.ActivePointBefore,
                BalanceAfter = tierMutation.ActivePointAfter,
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
