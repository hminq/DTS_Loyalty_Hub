namespace Core.UseCases.CustomerVouchers.Results;

public sealed record CustomerVoucherResult(
    Guid CusVoucherId,
    CustomerInfoResult CusInfo,
    string VoucherDefName,
    string VoucherDefRewardType,
    bool IsExpired);
