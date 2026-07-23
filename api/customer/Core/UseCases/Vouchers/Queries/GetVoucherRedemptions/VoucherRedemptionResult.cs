namespace Core.UseCases.Vouchers.Queries.GetVoucherRedemptions;

public sealed record VoucherRedemptionResult(
    Guid VoucherRedemptionId,
    Guid CustomerVoucherId,
    Guid VoucherDefinitionId,
    string VoucherDefinitionName,
    string? VoucherDefinitionDescription,
    Guid? CampaignId,
    string? CampaignName,
    DateTime ValidFrom,
    DateTime ValidTo,
    DateTime RedeemAt);
