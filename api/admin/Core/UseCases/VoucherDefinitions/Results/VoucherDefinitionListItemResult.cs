namespace Core.UseCases.VoucherDefinitions.Results;

public sealed record VoucherDefinitionListItemResult(
    Guid VoucherDefinitionId,
    string? Code,
    string Name,
    string RewardType,
    decimal? RewardValue,
    string PublishType,
    int TotalStock,
    int RemainingStock,
    DateTime CreatedAt,
    DateTime? DeletedAt);
