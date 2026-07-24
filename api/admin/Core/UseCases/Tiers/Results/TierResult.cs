namespace Core.UseCases.Tiers.Results;

public sealed record TierListItemResult(
    Guid TierConfigId,
    string Name,
    decimal PointsRequired,
    int CycleMonth,
    int Priority);

public sealed record TierDetailResult(
    Guid TierConfigId,
    string Name,
    decimal PointsRequired,
    int CycleMonth,
    int Priority,
    DateTime CreatedAt);

public sealed record TierResult(
    Guid TierConfigId,
    string Name,
    decimal PointsRequired,
    int CycleMonth,
    int Priority);
