using Core.Abstractions;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Vouchers.Queries.GetCustomerVouchers;

public sealed class GetCustomerVouchersQueryHandler
    : IRequestHandler<GetCustomerVouchersQuery, PagedResult<CustomerVoucherResult>>
{
    private readonly ICustomerVoucherRepository _repository;

    public GetCustomerVouchersQueryHandler(ICustomerVoucherRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<CustomerVoucherResult>> Handle(
        GetCustomerVouchersQuery request,
        CancellationToken cancellationToken)
    {
        return _repository.GetPagedVouchersAsync(
            request.CustomerId,
            NormalizePage(request.Page),
            NormalizePageSize(request.PageSize),
            NormalizeText(request.Name),
            NormalizeUppercase(request.RewardType),
            NormalizeDateTime(request.RedeemAtFrom),
            NormalizeDateTime(request.RedeemAtTo),
            NormalizeDateTime(request.CurrentTime)!.Value,
            cancellationToken);
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) =>
        pageSize is < 1 or > 100 ? 20 : pageSize;

    private static string? NormalizeText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeUppercase(string? value) =>
        NormalizeText(value)?.ToUpperInvariant();

    private static DateTime? NormalizeDateTime(DateTime? value) =>
        value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}
