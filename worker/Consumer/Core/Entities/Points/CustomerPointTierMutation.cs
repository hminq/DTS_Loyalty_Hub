namespace Consumer.Core.Entities.Points;

public sealed record CustomerPointTierMutation(
    Guid CustomerId,
    Guid? TierId,
    decimal CurrentTierPoint,
    Guid? NextTierId,
    decimal NextTierPoint,
    DateTime? StartTier,
    DateTime? ExpiredTier,
    decimal ActivePointBefore,
    decimal ActivePointAfter,
    decimal LifetimePointAfter);
