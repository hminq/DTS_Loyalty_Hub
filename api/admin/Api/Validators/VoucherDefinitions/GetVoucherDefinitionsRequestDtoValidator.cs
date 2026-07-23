using Api.Dtos.Requests.VoucherDefinitions;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.VoucherDefinitions;

public sealed class GetVoucherDefinitionsRequestDtoValidator
    : AbstractValidator<GetVoucherDefinitionsRequestDto>
{
    public GetVoucherDefinitionsRequestDtoValidator()
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

        RuleFor(request => request.Keyword)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.Keyword is not null)
            .OverridePropertyName("keyword");

        RuleFor(request => request.RewardType)
            .Cascade(CascadeMode.Stop)
            .Must(VoucherRewardTypes.IsDefined!)
            .WithErrorCode("VOUCHER_REWARD_TYPE_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.RewardType))
            .OverridePropertyName("rewardType");

        RuleFor(request => request.ValidityType)
            .Cascade(CascadeMode.Stop)
            .Must(VoucherValidityTypes.IsDefined!)
            .WithErrorCode("VOUCHER_VALIDITY_TYPE_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.ValidityType))
            .OverridePropertyName("validityType");

        RuleFor(request => request.PublishType)
            .Cascade(CascadeMode.Stop)
            .Must(VoucherPublishTypes.IsDefined!)
            .WithErrorCode("VOUCHER_PUBLISH_TYPE_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.PublishType))
            .OverridePropertyName("publishType");
    }
}
