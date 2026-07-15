namespace Core.UseCases.Tiers.Results;

public sealed record TierResult(
    Guid TierConfigId,
    string Name,
    decimal PointsRequired,
    int CycleMonth,
    int Priority);