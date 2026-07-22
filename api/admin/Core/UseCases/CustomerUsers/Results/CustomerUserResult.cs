namespace Core.UseCases.CustomerUsers.Results;

public sealed record CustomerUserListItemResult(
    Guid CustomerId,
    Guid UserId,
    string Username,
    string Email,
    string FullName,
    string? PhoneNumber,
    Guid? TierId,
    string? TierName,
    string Status,
    DateTime CreatedAt);

public sealed record CustomerUserDetailResult(
    Guid CustomerId,
    Guid UserId,
    string Username,
    string Email,
    string FullName,
    string? PhoneNumber,
    string Status,
    DateTime CreatedAt,
    decimal CurrentTierPoint,
    decimal NextTierPoint,
    CustomerUserTierResult? Tier);

public sealed record CustomerUserTierResult(
    Guid TierId,
    string Name,
    decimal PointsRequired,
    int CycleMonth,
    int Priority);

public sealed record CustomerUserPointsResult(
    Guid CustomerId,
    decimal CurrentTierPoint,
    decimal NextTierPoint,
    CustomerUserTierResult? Tier,
    decimal ActivePoint,
    decimal LockedPoint,
    decimal LifetimePoint,
    decimal SpentPoint,
    decimal ExpiredPoint,
    DateTime? UpdatedAt);

public sealed record CustomerVoucherResult(
    Guid CustomerVoucherId,
    Guid VoucherDefinitionId,
    string VoucherDefinitionName,
    Guid? VoucherPoolId,
    string VoucherCode,
    DateTime ValidFrom,
    DateTime ValidTo,
    int RemainingCount,
    DateTime ReceivedAt);

public sealed record CustomerVoucherRedemptionResult(
    Guid VoucherRedemptionId,
    Guid CustomerVoucherId,
    Guid VoucherDefinitionId,
    string VoucherDefinitionName,
    Guid? VoucherPoolId,
    string VoucherCode,
    Guid? CampaignId,
    string? CampaignName,
    Guid? CampaignSessionId,
    Guid? ActionId,
    string? ActionType,
    string? SourceEventId,
    DateTime RedeemedAt);

public sealed record CustomerPointTransactionResult(
    Guid PointTransactionId,
    string TransactionType,
    decimal Amount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    Guid? CampaignId,
    string? CampaignName,
    Guid? CampaignSessionId,
    Guid? ActionId,
    string? ActionType,
    string? SourceEventId,
    DateTime CreatedAt);
