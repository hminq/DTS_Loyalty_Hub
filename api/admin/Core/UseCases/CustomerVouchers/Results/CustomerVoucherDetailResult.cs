namespace Core.UseCases.CustomerVouchers.Results;

public sealed record CustomerVoucherDetailResult(
    Guid CusVoucherId,
    CustomerInfoResult CusInfo,
    Guid VoucherDefId,
    string VoucherDefName,
    string? VoucherDefDescription,
    string VoucherDefRewardType,
    string? VoucherDefBannerImgUrl,
    string VoucherDefGenerationType,
    DateTime ValidFrom,
    DateTime ValidTo,
    DateTime RedeemAt);
