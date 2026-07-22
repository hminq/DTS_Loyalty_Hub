using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Results;

namespace Core.Abstractions;

public interface ICustomerUserRepository
{
    Task<PagedResult<CustomerUserListItemResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? status,
        Guid? tierId,
        CancellationToken ct = default);

    Task<CustomerUserDetailResult?> GetByIdAsync(
        Guid customerId,
        CancellationToken ct = default);

    Task<CustomerUserPointsResult?> GetPointsAsync(
        Guid customerId,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        Guid customerId,
        CancellationToken ct = default);

    Task<PagedResult<CustomerVoucherResult>> GetVouchersAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<PagedResult<CustomerVoucherRedemptionResult>> GetVoucherRedemptionsAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<PagedResult<CustomerPointTransactionResult>> GetPointTransactionsAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
