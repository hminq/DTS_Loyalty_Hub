using Core.UseCases.Common;
using Core.UseCases.CustomerVouchers.Results;
using MediatR;

namespace Core.UseCases.CustomerVouchers.Queries.GetCustomerRedeems;

public sealed record GetCustomerRedeemsQuery(
    int Page,
    int PageSize,
    string? VoucherKeyword,
    string? RewardType,
    DateTime? RedeemAtFrom,
    DateTime? RedeemAtTo,
    string? CampaignName,
    string? UserKeyword)
    : IRequest<PagedResult<CustomerRedeemResult>>;
