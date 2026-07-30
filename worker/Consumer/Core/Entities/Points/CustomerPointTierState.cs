namespace Consumer.Core.Entities.Points;

public sealed record CustomerPointTierState(
    Guid CustomerId,
    Guid? TierId,
    decimal CurrentTierPoint,
    decimal NextTierPoint,
    DateTime? StartTier,
    DateTime? ExpiredTier,
    Guid? NextTierId,
    decimal ActivePoint,
    decimal LifetimePoint);
