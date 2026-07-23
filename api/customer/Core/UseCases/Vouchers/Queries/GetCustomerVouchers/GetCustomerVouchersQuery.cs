using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Vouchers.Queries.GetCustomerVouchers;

public sealed record GetCustomerVouchersQuery(
    Guid CustomerId,
    int Page,
    int PageSize,
    string? Name,
    string? RewardType,
    DateTime? RedeemAtFrom,
    DateTime? RedeemAtTo,
    DateTime CurrentTime)
    : IRequest<PagedResult<CustomerVoucherResult>>;
