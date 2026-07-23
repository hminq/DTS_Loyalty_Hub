using Api.Dtos.Requests.CustomerVouchers;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.CustomerVouchers;

public sealed class GetCustomerRedeemsRequestDtoValidator
    : AbstractValidator<GetCustomerRedeemsRequestDto>
{
    public GetCustomerRedeemsRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.VoucherKeyword)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.VoucherKeyword is not null)
            .OverridePropertyName("voucherKeyword");

        RuleFor(request => request.RewardType)
            .Must(value => value is null || VoucherRewardTypes.IsDefined(value))
            .WithErrorCode("VOUCHER_REWARD_TYPE_INVALID")
            .OverridePropertyName("rewardType");

        RuleFor(request => request.CampaignName)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.CampaignName is not null)
            .OverridePropertyName("campaignName");

        RuleFor(request => request.UserKeyword)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.UserKeyword is not null)
            .OverridePropertyName("userKeyword");

        RuleFor(request => request)
            .Must(request =>
                !request.RedeemAtFrom.HasValue ||
                !request.RedeemAtTo.HasValue ||
                request.RedeemAtFrom.Value <= request.RedeemAtTo.Value)
            .WithErrorCode("REDEEM_DATE_RANGE_INVALID")
            .OverridePropertyName("redeemAtFrom");
    }
}
