using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Vouchers.Queries.GetVoucherRedemptions;

public sealed record GetVoucherRedemptionsQuery(
    Guid CustomerId,
    int Page,
    int PageSize,
    string? VoucherKeyword,
    string? RewardType,
    DateTime? RedeemAtFrom,
    DateTime? RedeemAtTo,
    string? CampaignName)
    : IRequest<PagedResult<VoucherRedemptionResult>>;
