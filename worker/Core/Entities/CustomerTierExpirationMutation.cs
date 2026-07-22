namespace Core.Entities;

public sealed record CustomerTierExpirationMutation(
    Guid CustomerId,
    Guid TierConfigId,
    decimal CurrentTierPoint,
    DateTime StartTier,
    DateTime ExpiredTier,
    Guid? NextTierConfigId,
    decimal NextTierPoint,
    decimal TierPointBefore);
