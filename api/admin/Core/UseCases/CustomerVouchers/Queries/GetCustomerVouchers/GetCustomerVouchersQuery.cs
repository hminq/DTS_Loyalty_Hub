using Core.UseCases.Common;
using Core.UseCases.CustomerVouchers.Results;
using MediatR;

namespace Core.UseCases.CustomerVouchers.Queries.GetCustomerVouchers;

public sealed record GetCustomerVouchersQuery(
    int Page,
    int PageSize,
    string? VoucherKeyword,
    string? RewardType,
    DateTime? RedeemAtFrom,
    DateTime? RedeemAtTo,
    string? UserKeyword,
    DateTime CurrentTime)
    : IRequest<PagedResult<CustomerVoucherResult>>;
