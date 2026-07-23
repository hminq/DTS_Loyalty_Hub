using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerVouchers;
using Core.UseCases.CustomerVouchers.Results;
using MediatR;

namespace Core.UseCases.CustomerVouchers.Handlers;

public sealed class GetCustomerVouchersQueryHandler
    : IRequestHandler<GetCustomerVouchersQuery, PagedResult<CustomerVoucherResult>>
{
    private const int MaxPageSize = 100;
    private readonly ICustomerVoucherRepository _customerVoucherRepository;

    public GetCustomerVouchersQueryHandler(ICustomerVoucherRepository customerVoucherRepository)
    {
        _customerVoucherRepository = customerVoucherRepository;
    }

    public Task<PagedResult<CustomerVoucherResult>> Handle(
        GetCustomerVouchersQuery request,
        CancellationToken ct)
    {
        ValidatePaging(request.Page, request.PageSize);
        ValidateDateRange(request.RedeemAtFrom, request.RedeemAtTo);

        var rewardType = NormalizeRewardType(request.RewardType);

        return _customerVoucherRepository.GetPagedVouchersAsync(
            request.Page,
            request.PageSize,
            NormalizeKeyword(request.VoucherKeyword),
            rewardType,
            request.RedeemAtFrom,
            request.RedeemAtTo,
            NormalizeKeyword(request.UserKeyword),
            request.CurrentTime,
            ct);
    }

    private static string? NormalizeKeyword(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? NormalizeRewardType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalizedValue = VoucherRewardTypes.Normalize(value);
        if (!VoucherRewardTypes.IsDefined(normalizedValue))
        {
            throw new DomainException(
                "VOUCHER_REWARD_TYPE_INVALID",
                DomainErrorType.Validation);
        }

        return normalizedValue;
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new DomainException("PAGE_INVALID", DomainErrorType.Validation);
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new DomainException("PAGE_SIZE_INVALID", DomainErrorType.Validation);
        }
    }

    private static void ValidateDateRange(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            throw new DomainException(
                "REDEEM_DATE_RANGE_INVALID",
                DomainErrorType.Validation);
        }
    }
}
