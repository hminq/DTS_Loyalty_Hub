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

    public async Task ApplyBatchAsync(
        IReadOnlyList<CustomerTierExpirationMutation> mutations,
        DateTime processedAt,
        CancellationToken cancellationToken)
    {
        if (mutations.Count == 0)
        {
            return;
        }

        var customerIds = mutations
            .Select(mutation => mutation.CustomerId)
            .Distinct()
            .ToArray();

        var customersById = _dbContext.ChangeTracker
            .Entries<Customer>()
            .Where(entry => customerIds.Contains(entry.Entity.CustomerId))
            .Select(entry => entry.Entity)
            .ToDictionary(customer => customer.CustomerId);

        if (customersById.Count != customerIds.Length)
        {
            throw new InvalidOperationException(
                "Not all customers in the tier expiration batch are tracked by the current transaction.");
        }

        var customerPointsByCustomerId = (await _dbContext.CustomerPoints
                .FromSqlInterpolated($$"""
                    SELECT *
                    FROM customer_points
                    WHERE customer_id = ANY ({{customerIds}})
                    FOR UPDATE
                    """)
                .ToListAsync(cancellationToken))
            .ToDictionary(customerPoint => customerPoint.CustomerId);

        foreach (var mutation in mutations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var customer = customersById[mutation.CustomerId];
            customer.TierId = mutation.TierConfigId;
            customer.CurrentTierPoint = mutation.CurrentTierPoint;
            customer.StartTier = mutation.StartTier;
            customer.ExpiredTier = mutation.ExpiredTier;
            customer.NextTierId = mutation.NextTierConfigId;
            customer.NextTierPoint = mutation.NextTierPoint;

            if (!customerPointsByCustomerId.TryGetValue(mutation.CustomerId, out var customerPoint))
            {
                continue;
            }

            ApplyPointLoss(customerPoint, mutation, processedAt);
        }
    }

    private void ApplyPointLoss(
        CustomerPoint customerPoint,
        CustomerTierExpirationMutation mutation,
        DateTime processedAt)
    {
        var activePointBefore = customerPoint.ActivePoint;

        customerPoint.ExpiredPoint += activePointBefore;
        customerPoint.ActivePoint = 0;
        customerPoint.UpdatedAt = processedAt;

        if (activePointBefore != 0)
        {
            AddPointTransaction(
                mutation.CustomerId,
                PointTransactionTypes.ActivePointReset,
                PointResetAmounts.Calculate(activePointBefore, 0),
                activePointBefore,
                0,
                processedAt);
        }

        var tierPointChange = PointResetAmounts.Calculate(
            mutation.TierPointBefore,
            mutation.CurrentTierPoint);

        if (tierPointChange != 0)
        {
            AddPointTransaction(
                mutation.CustomerId,
                PointTransactionTypes.TierPointReset,
                tierPointChange,
                mutation.TierPointBefore,
                mutation.CurrentTierPoint,
                processedAt);
        }
    }

    private void AddPointTransaction(
        Guid customerId,
        string transactionType,
        decimal amount,
        decimal balanceBefore,
        decimal balanceAfter,
        DateTime createdAt)
    {
        _dbContext.PointTransactions.Add(new PointTransaction
        {
            PointTransactionId = Guid.NewGuid(),
            CustomerId = customerId,
            CampaignId = null,
            CampaignSessionId = null,
            ActionId = null,
            SourceEventId = null,
            TransactionType = transactionType,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            CreatedAt = createdAt,
        });
    }
}
