namespace Core.Entities;

public sealed record TierConfiguration(
    Guid TierConfigId,
    decimal PointsRequired,
    int CycleMonth,
    int Priority);
