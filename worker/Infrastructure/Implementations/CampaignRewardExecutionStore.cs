using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class CampaignRewardExecutionStore : ICampaignRewardExecutionStore
{
    private const decimal MaximumNumeric18Scale2 = 9999999999999999.99m;

    private readonly LoyaltyHubDbContext _dbContext;

    public CampaignRewardExecutionStore(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<CampaignRewardProcessingContext?> LockProcessingAsync(
        Guid eventCampaignProcessingId,
        CancellationToken cancellationToken = default)
    {
        var child = await _dbContext.EventCampaignProcessings
            .FromSqlInterpolated($$"""
                SELECT *
                FROM event_campaign_processings
                WHERE event_campaign_processing_id = {{eventCampaignProcessingId}}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);

        if (child is null)
        {
            return null;
        }

        var campaign = await _dbContext.Campaigns
            .FromSqlInterpolated($$"""
                SELECT *
                FROM campaigns
                WHERE campaign_id = {{child.CampaignId}}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
        var session = await _dbContext.CampaignSessions
            .FromSqlInterpolated($$"""
                SELECT *
                FROM campaign_sessions
                WHERE campaign_session_id = {{child.CampaignSessionId}}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
        var eventProcessing = await _dbContext.EventProcessings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                processing => processing.EventId == child.EventId,
                cancellationToken);

        if (eventProcessing is null || campaign is null || session is null)
        {
            throw PersistenceFailure();
        }

        return new CampaignRewardProcessingContext(
            child.EventCampaignProcessingId,
            child.EventId,
            child.CampaignId,
            child.CampaignSessionId,
            child.EventCustomerId,
            child.Status,
            child.AttemptCount,
            child.OutcomeCode,
            eventProcessing.EventType,
            eventProcessing.RoutingKey,
            eventProcessing.OccurredAt,
            eventProcessing.Payload,
            eventProcessing.PayloadHash,
            campaign.EventType,
            campaign.Condition,
            campaign.Status,
            campaign.UserLimitTotal,
            campaign.UserLimitSession,
            session.CampaignId,
            session.Status,
            session.SessionStart,
            session.SessionEnd);
    }

    public async Task<IReadOnlyList<Guid>> LockCustomersAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default)
    {
        if (customerIds.Count == 0)
        {
            return [];
        }

        var orderedIds = customerIds.Distinct().Order().ToArray();
        var customers = await _dbContext.Customers
            .FromSqlInterpolated($$"""
                SELECT *
                FROM customer
                WHERE customer_id = ANY ({{orderedIds}})
                ORDER BY customer_id
                FOR UPDATE
                """)
            .ToArrayAsync(cancellationToken);

        return customers.Select(customer => customer.CustomerId).ToArray();
    }

    public async Task<CampaignUsageCounts> GetCampaignUsageCountsAsync(
        Guid campaignId,
        Guid campaignSessionId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var totalUsed = await _dbContext.CampaignUsages
            .Where(usage =>
                usage.CampaignId == campaignId &&
                usage.CustomerId == customerId)
            .Select(usage => usage.EventCampaignProcessingId)
            .Distinct()
            .CountAsync(cancellationToken);

        var sessionUsed = await _dbContext.CampaignUsages
            .Where(usage =>
                usage.CampaignId == campaignId &&
                usage.CampaignSessionId == campaignSessionId &&
                usage.CustomerId == customerId)
            .Select(usage => usage.EventCampaignProcessingId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new CampaignUsageCounts(totalUsed, sessionUsed);
    }

    public async Task<IReadOnlyList<CampaignRewardAction>> LockActionsAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var actions = await _dbContext.Actions
            .FromSqlInterpolated($$"""
                SELECT *
                FROM actions
                WHERE reference_type = {{ActionReferenceTypes.Campaign}}
                  AND reference_id = {{campaignId}}
                ORDER BY execute_order, action_id
                FOR UPDATE
                """)
            .ToArrayAsync(cancellationToken);

        return actions
            .Select(action => new CampaignRewardAction(
                action.ActionId,
                action.ActionType,
                action.ActionConfig,
                action.ExecuteOrder,
                action.TotalCount,
                action.SessionCount,
                action.UsedCount))
            .ToArray();
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

    public async Task<IReadOnlyDictionary<Guid, int>> LockActionUsagesAsync(
        IReadOnlyCollection<Guid> actionIds,
        Guid campaignSessionId,
        CancellationToken cancellationToken = default)
    {
        if (actionIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var orderedIds = actionIds.Distinct().Order().ToArray();
        var usages = await _dbContext.ActionUsages
            .FromSqlInterpolated($$"""
                SELECT *
                FROM action_usage
                WHERE campaign_session_id = {{campaignSessionId}}
                  AND action_id = ANY ({{orderedIds}})
                ORDER BY action_id
                FOR UPDATE
                """)
            .ToArrayAsync(cancellationToken);

        return usages.ToDictionary(usage => usage.ActionId, usage => usage.UsedCount);
    }

    public void ApplyPointReward(PointRewardMutation mutation)
    {
        var action = _dbContext.Actions.Local.SingleOrDefault(
            item => item.ActionId == mutation.ActionId)
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

        var actionUsage = _dbContext.ActionUsages.Local.SingleOrDefault(
            usage =>
                usage.ActionId == mutation.ActionId &&
                usage.CampaignSessionId == mutation.CampaignSessionId);

        if (actionUsage is null)
        {
            actionUsage = new ActionUsage
            {
                ActionUsageId = mutation.ActionUsageId,
                ActionId = mutation.ActionId,
                CampaignSessionId = mutation.CampaignSessionId,
                UsedCount = 0,
                CreatedAt = mutation.CreatedAt
            };
            _dbContext.ActionUsages.Add(actionUsage);
        }

        action.UsedCount = checked(action.UsedCount + 1);
        actionUsage.UsedCount = checked(actionUsage.UsedCount + 1);

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

        _dbContext.CampaignUsages.Add(
            new CampaignUsage
            {
                CampaignUsageId = mutation.CampaignUsageId,
                CampaignId = mutation.CampaignId,
                CampaignSessionId = mutation.CampaignSessionId,
                CustomerId = mutation.CustomerId,
                ActionId = mutation.ActionId,
                EventCampaignProcessingId = mutation.EventCampaignProcessingId,
                CreatedAt = mutation.CreatedAt
            });
    }

    public void MarkSkipped(
        Guid eventCampaignProcessingId,
        string outcomeCode,
        DateTime processedAt)
    {
        var child = GetLockedChild(eventCampaignProcessingId);
        child.Status = EventCampaignProcessingStatuses.Skipped;
        child.AttemptCount++;
        child.OutcomeCode = outcomeCode;
        child.LastError = null;
        child.ProcessedAt = processedAt;
    }

    public void MarkCompleted(
        Guid eventCampaignProcessingId,
        DateTime processedAt)
    {
        var child = GetLockedChild(eventCampaignProcessingId);
        child.Status = EventCampaignProcessingStatuses.Completed;
        child.AttemptCount++;
        child.OutcomeCode = null;
        child.LastError = null;
        child.ProcessedAt = processedAt;
    }

    private EventCampaignProcessing GetLockedChild(Guid eventCampaignProcessingId)
    {
        return _dbContext.EventCampaignProcessings.Local.SingleOrDefault(
            processing =>
                processing.EventCampaignProcessingId == eventCampaignProcessingId)
            ?? throw PersistenceFailure();
    }

    private static CampaignProcessingException PersistenceFailure()
    {
        return new CampaignProcessingException(
            CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
            retriable: true);
    }
}
