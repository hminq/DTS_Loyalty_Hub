using Consumer.Core.Entities.Points;
using Consumer.Core.Exceptions;
using Consumer.Core.Services;
using FluentAssertions;

namespace Consumer.Core.Tests.Points;

public sealed class CustomerTierProgressionServiceTests
{
    private static readonly DateTime ExecutedAt =
        new(2026, 7, 30, 9, 15, 0, DateTimeKind.Utc);

    private readonly CustomerTierProgressionService _service = new();

    [Fact]
    public void Calculate_UnrankedBelowFirstThreshold_KeepsTierNullAndPointsToFirstTier()
    {
        var first = Tier(1, 100, 6);
        var result = _service.Calculate(Unranked(currentTierPoint: 0), [first], 50, ExecutedAt);

        result.TierId.Should().BeNull();
        result.CurrentTierPoint.Should().Be(50);
        result.NextTierId.Should().Be(first.TierConfigId);
        result.NextTierPoint.Should().Be(100);
        result.StartTier.Should().BeNull();
        result.ExpiredTier.Should().BeNull();
        result.ActivePointAfter.Should().Be(50);
        result.LifetimePointAfter.Should().Be(50);
    }

    [Fact]
    public void Calculate_UnrankedReachesFirstTier_PromotesAndStartsTierCycle()
    {
        var first = Tier(1, 100, 6);
        var second = Tier(2, 500, 12);
        var result = _service.Calculate(Unranked(currentTierPoint: 60), [first, second], 40, ExecutedAt);

        result.TierId.Should().Be(first.TierConfigId);
        result.CurrentTierPoint.Should().Be(100);
        result.NextTierId.Should().Be(second.TierConfigId);
        result.NextTierPoint.Should().Be(500);
        result.StartTier.Should().Be(ExecutedAt);
        result.ExpiredTier.Should().Be(ExecutedAt.AddMonths(6));
    }

    [Fact]
    public void Calculate_UnrankedJumpAcrossMultipleTiers_SelectsHighestEligibleTier()
    {
        var tiers = StandardTiers();
        var result = _service.Calculate(Unranked(currentTierPoint: 0), tiers, 1_250, ExecutedAt);

        result.TierId.Should().Be(tiers[2].TierConfigId);
        result.NextTierId.Should().BeNull();
        result.NextTierPoint.Should().Be(tiers[2].PointsRequired);
        result.CurrentTierPoint.Should().Be(1_250);
    }

    [Fact]
    public void Calculate_RankedWithoutPromotion_PreservesExistingTierDates()
    {
        var tiers = StandardTiers();
        var startTier = ExecutedAt.AddMonths(-1);
        var expiredTier = ExecutedAt.AddMonths(5);
        var state = Ranked(tiers[0], currentTierPoint: 120) with
        {
            StartTier = startTier,
            ExpiredTier = expiredTier,
            NextTierId = tiers[1].TierConfigId,
            NextTierPoint = tiers[1].PointsRequired
        };

        var result = _service.Calculate(state, tiers, 100, ExecutedAt);

        result.TierId.Should().Be(tiers[0].TierConfigId);
        result.CurrentTierPoint.Should().Be(220);
        result.StartTier.Should().Be(startTier);
        result.ExpiredTier.Should().Be(expiredTier);
        result.NextTierId.Should().Be(tiers[1].TierConfigId);
    }

    [Fact]
    public void Calculate_RankedPromotion_ResetsTierDatesUsingPromotedCycle()
    {
        var tiers = StandardTiers();
        var result = _service.Calculate(Ranked(tiers[0], currentTierPoint: 450), tiers, 75, ExecutedAt);

        result.TierId.Should().Be(tiers[1].TierConfigId);
        result.CurrentTierPoint.Should().Be(525);
        result.NextTierId.Should().Be(tiers[2].TierConfigId);
        result.StartTier.Should().Be(ExecutedAt);
        result.ExpiredTier.Should().Be(ExecutedAt.AddMonths(tiers[1].CycleMonth));
    }

    [Fact]
    public void Calculate_RankedMultipleTierPromotion_PreservesSurplusTierPoints()
    {
        var tiers = StandardTiers();
        var result = _service.Calculate(Ranked(tiers[0], currentTierPoint: 450), tiers, 800, ExecutedAt);

        result.TierId.Should().Be(tiers[2].TierConfigId);
        result.CurrentTierPoint.Should().Be(1_250);
        result.NextTierId.Should().BeNull();
        result.NextTierPoint.Should().Be(tiers[2].PointsRequired);
    }

    [Fact]
    public void Calculate_AlreadyHighestTier_KeepsHighestTierAndThreshold()
    {
        var tiers = StandardTiers();
        var state = Ranked(tiers[2], currentTierPoint: 1_500) with
        {
            NextTierId = Guid.NewGuid(),
            NextTierPoint = 9_999
        };

        var result = _service.Calculate(state, tiers, 25, ExecutedAt);

        result.TierId.Should().Be(tiers[2].TierConfigId);
        result.NextTierId.Should().BeNull();
        result.NextTierPoint.Should().Be(tiers[2].PointsRequired);
        result.CurrentTierPoint.Should().Be(1_525);
    }

    [Fact]
    public void Calculate_EmptyTierCatalog_StillReturnsPointMutation()
    {
        var tierId = Guid.NewGuid();
        var nextTierId = Guid.NewGuid();
        var state = new CustomerPointTierState(
            Guid.NewGuid(),
            tierId,
            90,
            100,
            ExecutedAt.AddMonths(-1),
            ExecutedAt.AddMonths(5),
            nextTierId,
            10,
            20);

        var result = _service.Calculate(state, [], 15, ExecutedAt);

        result.TierId.Should().Be(tierId);
        result.NextTierId.Should().Be(nextTierId);
        result.CurrentTierPoint.Should().Be(105);
        result.ActivePointBefore.Should().Be(10);
        result.ActivePointAfter.Should().Be(25);
        result.LifetimePointAfter.Should().Be(35);
    }

    [Fact]
    public void Calculate_ConfigurationDriftDoesNotDowngradePersistedTier()
    {
        var high = Tier(3, 1_000, 12);
        var low = Tier(1, 100, 6);
        var result = _service.Calculate(Ranked(high, currentTierPoint: 150), [low, high], 10, ExecutedAt);

        result.TierId.Should().Be(high.TierConfigId);
        result.CurrentTierPoint.Should().Be(160);
    }

    [Fact]
    public void Calculate_MissingPersistedTierInCatalog_KeepsExistingTierFields()
    {
        var missingTierId = Guid.NewGuid();
        var existingNextTierId = Guid.NewGuid();
        var state = new CustomerPointTierState(
            Guid.NewGuid(),
            missingTierId,
            150,
            250,
            ExecutedAt.AddMonths(-1),
            ExecutedAt.AddMonths(5),
            existingNextTierId,
            10,
            20);

        var result = _service.Calculate(state, [Tier(1, 100, 6)], 10, ExecutedAt);

        result.TierId.Should().Be(missingTierId);
        result.NextTierId.Should().Be(existingNextTierId);
        result.NextTierPoint.Should().Be(250);
        result.StartTier.Should().Be(state.StartTier);
        result.ExpiredTier.Should().Be(state.ExpiredTier);
    }

    [Theory]
    [InlineData("active")]
    [InlineData("lifetime")]
    [InlineData("tier")]
    public void Calculate_NumericOverflow_ThrowsCampaignProcessingException(string field)
    {
        const decimal nearMaximum = 9999999999999999.99m;
        var state = Unranked(
            currentTierPoint: field == "tier" ? nearMaximum : 0,
            activePoint: field == "active" ? nearMaximum : 0,
            lifetimePoint: field == "lifetime" ? nearMaximum : 0);

        var act = () => _service.Calculate(state, [], 0.01m, ExecutedAt);

        act.Should().Throw<CampaignProcessingException>();
    }

    private static CustomerPointTierState Unranked(
        decimal currentTierPoint,
        decimal activePoint = 0,
        decimal lifetimePoint = 0)
    {
        return new CustomerPointTierState(
            Guid.NewGuid(),
            null,
            currentTierPoint,
            0,
            null,
            null,
            null,
            activePoint,
            lifetimePoint);
    }

    private static CustomerPointTierState Ranked(
        TierProgressionConfiguration tier,
        decimal currentTierPoint)
    {
        return new CustomerPointTierState(
            Guid.NewGuid(),
            tier.TierConfigId,
            currentTierPoint,
            0,
            ExecutedAt.AddMonths(-1),
            ExecutedAt.AddMonths(5),
            null,
            0,
            0);
    }

    private static TierProgressionConfiguration[] StandardTiers()
    {
        return
        [
            Tier(1, 100, 6),
            Tier(2, 500, 9),
            Tier(3, 1_000, 12)
        ];
    }

    private static TierProgressionConfiguration Tier(
        int priority,
        decimal pointsRequired,
        int cycleMonths)
    {
        return new TierProgressionConfiguration(
            Guid.NewGuid(),
            pointsRequired,
            cycleMonths,
            priority);
    }
}
