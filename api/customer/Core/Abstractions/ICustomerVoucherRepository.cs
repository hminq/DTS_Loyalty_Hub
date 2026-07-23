using Core.UseCases.Common;
using Core.UseCases.Vouchers.Queries.GetCustomerVoucherDetail;
using Core.UseCases.Vouchers.Queries.GetCustomerVouchers;
using Core.UseCases.Vouchers.Queries.GetVoucherRedemptions;

namespace Core.Abstractions;

public interface ICustomerVoucherRepository
{
    Task<PagedResult<CustomerVoucherResult>> GetPagedVouchersAsync(
        Guid customerId,
        int page,
        int pageSize,
        string? voucherKeyword,
        string? rewardType,
        DateTime? redeemAtFrom,
        DateTime? redeemAtTo,
        DateTime currentTime,
        CancellationToken cancellationToken);

    Task<CustomerVoucherDetailResult?> GetVoucherDetailAsync(
        Guid customerId,
        Guid customerVoucherId,
        CancellationToken cancellationToken);

    Task<PagedResult<VoucherRedemptionResult>> GetPagedRedemptionsAsync(
        Guid customerId,
        int page,
        int pageSize,
        string? voucherKeyword,
        string? rewardType,
        DateTime? redeemAtFrom,
        DateTime? redeemAtTo,
        string? campaignName,
        CancellationToken cancellationToken);
}
