namespace Core.UseCases.Vouchers.Queries.GetCustomerVouchers;

public sealed record CustomerVoucherResult(
    Guid CustomerVoucherId,
    string VoucherDefinitionName,
    string VoucherDefinitionRewardType,
    bool IsExpired);
