using Core.UseCases.Common;
using Core.UseCases.CustomerVouchers.Results;

namespace Core.Abstractions;

public interface ICustomerVoucherRepository
{
    Task<PagedResult<CustomerVoucherResult>> GetPagedVouchersAsync(
        int page,
        int pageSize,
        string? voucherKeyword,
        string? rewardType,
        DateTime? redeemAtFrom,
        DateTime? redeemAtTo,
        string? userKeyword,
        DateTime currentTime,
        CancellationToken ct = default);

    Task<CustomerVoucherDetailResult?> GetVoucherDetailAsync(
        Guid customerVoucherId,
        CancellationToken ct = default);

    Task<PagedResult<CustomerRedeemResult>> GetPagedRedeemsAsync(
        int page,
        int pageSize,
        string? voucherKeyword,
        string? rewardType,
        DateTime? redeemAtFrom,
        DateTime? redeemAtTo,
        string? campaignName,
        string? userKeyword,
        CancellationToken ct = default);
}
