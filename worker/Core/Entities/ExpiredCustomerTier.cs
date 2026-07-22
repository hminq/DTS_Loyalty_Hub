namespace Core.Entities;

public sealed record ExpiredCustomerTier(
    Guid CustomerId,
    Guid TierConfigId,
    decimal CurrentTierPoint);
