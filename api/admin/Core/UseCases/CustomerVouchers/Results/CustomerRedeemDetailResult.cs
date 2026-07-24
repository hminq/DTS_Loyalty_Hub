namespace Core.UseCases.CustomerVouchers.Results;

public sealed record CustomerRedeemDetailResult(
    Guid VoucherRedemptionId,
    DateTime RedeemedAt,
    CustomerRedeemCustomerResult Customer,
    CustomerRedeemVoucherResult Voucher,
    CustomerRedeemIssuanceSourceResult IssuanceSource);

public sealed record CustomerRedeemCustomerResult(
    Guid CustomerId,
    string Username,
    string Email,
    string? Phone);

public sealed record CustomerRedeemVoucherResult(
    Guid CustomerVoucherId,
    Guid VoucherDefinitionId,
    Guid? VoucherPoolId,
    string Name,
    string? Description,
    string? BannerImageUrl,
    string VoucherCode,
    string RewardType,
    decimal? RewardValue,
    string GenerationType,
    DateTime ValidFrom,
    DateTime ValidTo);

public sealed record CustomerRedeemIssuanceSourceResult(
    Guid? CampaignId,
    string? CampaignName,
    string? CampaignEventType,
    Guid? CampaignSessionId,
    DateTime? SessionStart,
    DateTime? SessionEnd,
    string? SessionStatus,
    Guid? ActionId,
    string? ActionType);
