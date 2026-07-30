using Consumer.Core.Entities.Constants;
using Consumer.Core.Entities.Points;
using Consumer.Core.Exceptions;

namespace Consumer.Core.Services;

public sealed class CustomerTierProgressionService
{
    private const decimal MaximumNumeric18Scale2 = 9999999999999999.99m;

    public CustomerPointTierMutation Calculate(
        CustomerPointTierState state,
        IReadOnlyCollection<TierProgressionConfiguration> tiers,
        decimal amount,
        DateTime executedAt)
    {
        ArgumentNullException.ThrowIfNull(tiers);

        var activePointAfter = AddNumeric18Scale2(state.ActivePoint, amount);
        var lifetimePointAfter = AddNumeric18Scale2(state.LifetimePoint, amount);
        var currentTierPointAfter = AddNumeric18Scale2(state.CurrentTierPoint, amount);

        if (tiers.Count == 0)
        {
            return new CustomerPointTierMutation(
                state.CustomerId,
                state.TierId,
                currentTierPointAfter,
                state.NextTierId,
                state.NextTierPoint,
                state.StartTier,
                state.ExpiredTier,
                state.ActivePoint,
                activePointAfter,
                lifetimePointAfter);
        }

        var orderedTiers = tiers
            .OrderBy(tier => tier.Priority)
            .ToArray();
        var eligibleTier = orderedTiers
            .Where(tier => tier.PointsRequired <= currentTierPointAfter)
            .LastOrDefault();

        if (eligibleTier is null)
        {
            var firstTier = orderedTiers[0];
            return new CustomerPointTierMutation(
                state.CustomerId,
                state.TierId,
                currentTierPointAfter,
                firstTier.TierConfigId,
                firstTier.PointsRequired,
                state.StartTier,
                state.ExpiredTier,
                state.ActivePoint,
                activePointAfter,
                lifetimePointAfter);
        }

        var currentTier = state.TierId.HasValue
            ? orderedTiers.SingleOrDefault(tier => tier.TierConfigId == state.TierId.Value)
            : null;
        var shouldPromote = !state.TierId.HasValue ||
            currentTier is not null && eligibleTier.Priority > currentTier.Priority;

        if (!shouldPromote)
        {
            if (currentTier is null)
            {
                return new CustomerPointTierMutation(
                    state.CustomerId,
                    state.TierId,
                    currentTierPointAfter,
                    state.NextTierId,
                    state.NextTierPoint,
                    state.StartTier,
                    state.ExpiredTier,
                    state.ActivePoint,
                    activePointAfter,
                    lifetimePointAfter);
            }

            var retainedTier = currentTier;
            var nextTier = NextTierAfter(orderedTiers, retainedTier.Priority);
            return new CustomerPointTierMutation(
                state.CustomerId,
                state.TierId,
                currentTierPointAfter,
                nextTier?.TierConfigId,
                nextTier?.PointsRequired ?? orderedTiers[^1].PointsRequired,
                state.StartTier,
                state.ExpiredTier,
                state.ActivePoint,
                activePointAfter,
                lifetimePointAfter);
        }

        var promotedNextTier = NextTierAfter(orderedTiers, eligibleTier.Priority);
        return new CustomerPointTierMutation(
            state.CustomerId,
            eligibleTier.TierConfigId,
            currentTierPointAfter,
            promotedNextTier?.TierConfigId,
            promotedNextTier?.PointsRequired ?? orderedTiers[^1].PointsRequired,
            executedAt,
            executedAt.AddMonths(eligibleTier.CycleMonth),
            state.ActivePoint,
            activePointAfter,
            lifetimePointAfter);
    }

    private static TierProgressionConfiguration? NextTierAfter(
        IReadOnlyList<TierProgressionConfiguration> orderedTiers,
        int priority)
    {
        return orderedTiers.FirstOrDefault(tier => tier.Priority > priority);
    }

    private static decimal AddNumeric18Scale2(decimal current, decimal amount)
    {
        decimal result;
        try
        {
            result = checked(current + amount);
        }
        catch (OverflowException)
        {
            throw PersistenceFailure();
        }

        if (result > MaximumNumeric18Scale2)
        {
            throw PersistenceFailure();
        }

        return result;
    }

    private static CampaignProcessingException PersistenceFailure()
    {
        return new CampaignProcessingException(
            CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
            retriable: true);
    }
}
