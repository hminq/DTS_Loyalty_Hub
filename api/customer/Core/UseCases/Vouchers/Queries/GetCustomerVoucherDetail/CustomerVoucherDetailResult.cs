namespace Core.UseCases.Vouchers.Queries.GetCustomerVoucherDetail;

public sealed record CustomerVoucherDetailResult(
    Guid CustomerVoucherId,
    Guid VoucherDefinitionId,
    string VoucherDefinitionName,
    string? VoucherDefinitionDescription,
    string VoucherDefinitionRewardType,
    string? VoucherDefinitionBannerImageUrl,
    DateTime ValidFrom,
    DateTime ValidTo,
    DateTime RedeemAt);
