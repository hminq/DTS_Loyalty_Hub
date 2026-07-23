using Core.Abstractions;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Vouchers.Queries.GetVoucherRedemptions;

public sealed class GetVoucherRedemptionsQueryHandler
    : IRequestHandler<GetVoucherRedemptionsQuery, PagedResult<VoucherRedemptionResult>>
{
    private readonly ICustomerVoucherRepository _repository;

    public GetVoucherRedemptionsQueryHandler(ICustomerVoucherRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<VoucherRedemptionResult>> Handle(
        GetVoucherRedemptionsQuery request,
        CancellationToken cancellationToken)
    {
        return _repository.GetPagedRedemptionsAsync(
            request.CustomerId,
            request.Page < 1 ? 1 : request.Page,
            request.PageSize is < 1 or > 100 ? 20 : request.PageSize,
            NormalizeText(request.Name),
            NormalizeText(request.RewardType)?.ToUpperInvariant(),
            NormalizeDateTime(request.RedeemAtFrom),
            NormalizeDateTime(request.RedeemAtTo),
            NormalizeText(request.CampaignName),
            cancellationToken);
    }

    private static string? NormalizeText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime? NormalizeDateTime(DateTime? value) =>
        value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}
