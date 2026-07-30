using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Services;
using Consumer.Infrastructure.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Consumer.Infrastructure.Tests.Implementations;

public sealed class IssuePointExecutionStoreTests
{
    [Fact]
    public async Task ApplyIssuePoint_UpdatesPointAndTierStateAndAddsTransaction()
    {
        await using var dbContext = CreateDbContext();
        var executedAt = new DateTime(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc);
        var bronze = AddTier(dbContext, priority: 1, pointsRequired: 100, cycleMonth: 6);
        var silver = AddTier(dbContext, priority: 2, pointsRequired: 500, cycleMonth: 9);
        var customer = AddTrackedCustomer(dbContext, currentTierPoint: 450);
        var customerPoint = AddTrackedCustomerPoint(dbContext, customer.CustomerId, active: 75, lifetime: 200);
        await dbContext.SaveChangesAsync();
        var store = new IssuePointExecutionStore(dbContext, new CustomerTierProgressionService());
        await store.PrepareIssuePointExecutionAsync([], CancellationToken.None);

        store.ApplyIssuePoint(Mutation(customer.CustomerId, amount: 75, executedAt));

        customer.TierId.Should().Be(silver.TierConfigId);
        customer.CurrentTierPoint.Should().Be(525);
        customer.NextTierId.Should().BeNull();
        customer.NextTierPoint.Should().Be(silver.PointsRequired);
        customer.StartTier.Should().Be(executedAt);
        customer.ExpiredTier.Should().Be(executedAt.AddMonths(silver.CycleMonth));
        customerPoint.ActivePoint.Should().Be(150);
        customerPoint.LifetimePoint.Should().Be(275);
        var transaction = dbContext.PointTransactions.Local.Single();
        transaction.TransactionType.Should().Be(PointTransactionTypes.CampaignReward);
        transaction.BalanceBefore.Should().Be(75);
        transaction.BalanceAfter.Should().Be(150);
        bronze.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyIssuePoint_MultipleActionsForSameCustomer_UsesTrackedCumulativeState()
    {
        await using var dbContext = CreateDbContext();
        var executedAt = new DateTime(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc);
        var bronze = AddTier(dbContext, priority: 1, pointsRequired: 100, cycleMonth: 6);
        var silver = AddTier(dbContext, priority: 2, pointsRequired: 500, cycleMonth: 9);
        var customer = AddTrackedCustomer(dbContext, currentTierPoint: 50);
        AddTrackedCustomerPoint(dbContext, customer.CustomerId, active: 10, lifetime: 20);
        await dbContext.SaveChangesAsync();
        var store = new IssuePointExecutionStore(dbContext, new CustomerTierProgressionService());
        await store.PrepareIssuePointExecutionAsync([], CancellationToken.None);

        store.ApplyIssuePoint(Mutation(customer.CustomerId, amount: 75, executedAt));
        store.ApplyIssuePoint(Mutation(customer.CustomerId, amount: 400, executedAt.AddMinutes(1)));

        customer.TierId.Should().Be(silver.TierConfigId);
        customer.CurrentTierPoint.Should().Be(525);
        customer.NextTierId.Should().BeNull();
        customer.NextTierPoint.Should().Be(silver.PointsRequired);
        dbContext.CustomerPoints.Local.Single().ActivePoint.Should().Be(485);
        dbContext.PointTransactions.Local
            .OrderBy(transaction => transaction.CreatedAt)
            .Select(transaction => transaction.BalanceBefore)
            .Should()
            .Equal(10, 85);
        bronze.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyIssuePoint_EmptyTierCatalog_KeepsTierIdentityAndCreatesCustomerPoint()
    {
        await using var dbContext = CreateDbContext();
        var executedAt = new DateTime(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc);
        var existingTierId = Guid.NewGuid();
        var customer = AddTrackedCustomer(
            dbContext,
            currentTierPoint: 90,
            tierId: existingTierId,
            nextTierPoint: 250);
        await dbContext.SaveChangesAsync();
        var store = new IssuePointExecutionStore(dbContext, new CustomerTierProgressionService());
        await store.PrepareIssuePointExecutionAsync([], CancellationToken.None);

        store.ApplyIssuePoint(Mutation(customer.CustomerId, amount: 25, executedAt));

        customer.TierId.Should().Be(existingTierId);
        customer.CurrentTierPoint.Should().Be(115);
        customer.NextTierPoint.Should().Be(250);
        var customerPoint = dbContext.CustomerPoints.Local.Single();
        customerPoint.CustomerId.Should().Be(customer.CustomerId);
        customerPoint.ActivePoint.Should().Be(25);
        customerPoint.LifetimePoint.Should().Be(25);
    }

    [Fact]
    public void ApplyIssuePoint_MissingLockedCustomer_ThrowsWithoutPointTransaction()
    {
        using var dbContext = CreateDbContext();
        var store = new IssuePointExecutionStore(dbContext, new CustomerTierProgressionService());

        var act = () => store.ApplyIssuePoint(
            Mutation(Guid.NewGuid(), amount: 25, DateTime.UtcNow));

        act.Should().Throw<CampaignProcessingException>();
        dbContext.PointTransactions.Local.Should().BeEmpty();
    }

    private static LoyaltyHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LoyaltyHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new LoyaltyHubDbContext(options);
    }

    private static Customer AddTrackedCustomer(
        LoyaltyHubDbContext dbContext,
        decimal currentTierPoint,
        Guid? tierId = null,
        Guid? nextTierId = null,
        decimal nextTierPoint = 0)
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TierId = tierId,
            CurrentTierPoint = currentTierPoint,
            NextTierId = nextTierId,
            NextTierPoint = nextTierPoint,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Customers.Add(customer);
        return customer;
    }

    private static CustomerPoint AddTrackedCustomerPoint(
        LoyaltyHubDbContext dbContext,
        Guid customerId,
        decimal active,
        decimal lifetime)
    {
        var customerPoint = new CustomerPoint
        {
            CustomerPointId = Guid.NewGuid(),
            CustomerId = customerId,
            ActivePoint = active,
            LifetimePoint = lifetime,
            LockedPoint = 0,
            SpentPoint = 0,
            ExpiredPoint = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.CustomerPoints.Add(customerPoint);
        return customerPoint;
    }

    private static TiersConfig AddTier(
        LoyaltyHubDbContext dbContext,
        int priority,
        decimal pointsRequired,
        int cycleMonth)
    {
        var tier = new TiersConfig
        {
            TierConfigId = Guid.NewGuid(),
            Name = $"Tier {priority}",
            Priority = priority,
            PointsRequired = pointsRequired,
            CycleMonth = cycleMonth,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.TiersConfigs.Add(tier);
        return tier;
    }

    private static IssuePointMutation Mutation(
        Guid customerId,
        decimal amount,
        DateTime executedAt)
    {
        return new IssuePointMutation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            customerId,
            Guid.NewGuid(),
            amount,
            executedAt);
    }
}
