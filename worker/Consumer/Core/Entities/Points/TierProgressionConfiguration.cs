namespace Consumer.Core.Entities.Points;

public sealed record TierProgressionConfiguration(
    Guid TierConfigId,
    decimal PointsRequired,
    int CycleMonth,
    int Priority);
