namespace Core.UseCases.CustomerVouchers.Results;

public sealed record CustomerRedeemResult(
    Guid VoucherRedemptionId,
    CustomerInfoResult CusInfo,
    Guid CusVoucherId,
    Guid VoucherDefId,
    string VoucherDefName,
    string? VoucherDefDescription,
    Guid? CampaignId,
    string? CampaignName,
    DateTime ValidFrom,
    DateTime ValidTo,
    DateTime RedeemAt);
