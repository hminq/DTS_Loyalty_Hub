using Api.Dtos.Requests.Vouchers;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.Vouchers;

public sealed class CustomerVoucherFilterDtoValidator
    : AbstractValidator<CustomerVoucherFilterDto>
{
    public CustomerVoucherFilterDtoValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.VoucherKeyword)
            .MaximumLength(200)
            .WithErrorCode("VOUCHER_NAME_TOO_LONG")
            .When(request => request.VoucherKeyword is not null)
            .OverridePropertyName("voucherKeyword");

        RuleFor(request => request.RewardType!)
            .Must(VoucherRewardTypes.IsDefined)
            .WithErrorCode("VOUCHER_REWARD_TYPE_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.RewardType))
            .OverridePropertyName("rewardType");

        RuleFor(request => request.RedeemAtTo)
            .Must((request, redeemAtTo) =>
                !request.RedeemAtFrom.HasValue ||
                !redeemAtTo.HasValue ||
                request.RedeemAtFrom.Value <= redeemAtTo.Value)
            .WithErrorCode("REDEEM_DATE_RANGE_INVALID")
            .OverridePropertyName("redeemAtTo");
    }
}
