namespace Core.UseCases.VoucherDefinitions.Results;

public sealed record VoucherDefinitionResult(
    Guid VoucherDefinitionId,
    string? Code,
    string Name,
    string? Description,
    string? BannerImageUrl,
    string RewardType,
    decimal? RewardValue,
    string ValidityType,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int? DurationDay,
    string GenerationType,
    string PublishType,
    int TotalStock,
    int RemainingStock,
    DateTime CreatedAt,
    DateTime? DeletedAt,
    VoucherPoolProvisioningResult? PoolProvisioning = null);
