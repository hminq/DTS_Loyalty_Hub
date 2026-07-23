using Api.Dtos.Requests.VoucherDefinitions;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.VoucherDefinitions;

public sealed class CreateVoucherDefinitionRequestDtoValidator
    : AbstractValidator<CreateVoucherDefinitionRequestDto>
{
    public CreateVoucherDefinitionRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Code)
            .MaximumLength(200)
            .WithErrorCode("VOUCHER_CODE_TOO_LONG")
            .OverridePropertyName("code");

        RuleFor(request => request.Name)
            .NotEmpty()
            .WithErrorCode("VOUCHER_DEFINITION_NAME_REQUIRED")
            .MaximumLength(200)
            .WithErrorCode("VOUCHER_DEFINITION_NAME_TOO_LONG")
            .OverridePropertyName("name");

        RuleFor(request => request.RewardType)
            .NotEmpty()
            .WithErrorCode("VOUCHER_REWARD_TYPE_REQUIRED")
            .Must(VoucherRewardTypes.IsDefined)
            .WithErrorCode("VOUCHER_REWARD_TYPE_INVALID")
            .OverridePropertyName("rewardType");

        RuleFor(request => request.ValidityType)
            .NotEmpty()
            .WithErrorCode("VOUCHER_VALIDITY_TYPE_REQUIRED")
            .Must(VoucherValidityTypes.IsDefined)
            .WithErrorCode("VOUCHER_VALIDITY_TYPE_INVALID")
            .OverridePropertyName("validityType");

        RuleFor(request => request.GenerationType)
            .NotEmpty()
            .WithErrorCode("VOUCHER_GENERATION_TYPE_REQUIRED")
            .Must(VoucherGenerationTypes.IsDefined)
            .WithErrorCode("VOUCHER_GENERATION_TYPE_INVALID")
            .OverridePropertyName("generationType");

        RuleFor(request => request.PublishType)
            .NotEmpty()
            .WithErrorCode("VOUCHER_PUBLISH_TYPE_REQUIRED")
            .Must(VoucherPublishTypes.IsDefined)
            .WithErrorCode("VOUCHER_PUBLISH_TYPE_INVALID")
            .OverridePropertyName("publishType");

        RuleFor(request => request.TotalStock)
            .GreaterThan(0)
            .WithErrorCode("VOUCHER_TOTAL_STOCK_INVALID")
            .OverridePropertyName("totalStock");

        RuleFor(request => request.BannerImageUrl)
            .Must(key => key!.StartsWith(BannerUploadTypes.VoucherDefinitionBannerPrefix))
            .When(request => !string.IsNullOrWhiteSpace(request.BannerImageUrl))
            .WithErrorCode("VOUCHER_BANNER_IMAGE_KEY_INVALID")
            .OverridePropertyName("bannerImageUrl");

        RuleFor(request => request.DurationDay)
            .GreaterThan(0)
            .When(request => request.DurationDay.HasValue)
            .WithErrorCode("VOUCHER_DURATION_DAY_INVALID")
            .OverridePropertyName("durationDay");

        RuleFor(request => request.Code)
            .NotEmpty()
            .When(request => IsType(request.PublishType, VoucherPublishTypes.Public))
            .WithErrorCode("VOUCHER_CODE_REQUIRED")
            .OverridePropertyName("code");

        RuleFor(request => request.Code)
            .Empty()
            .When(request => IsType(request.PublishType, VoucherPublishTypes.Private))
            .WithErrorCode("VOUCHER_CODE_MUST_BE_EMPTY")
            .OverridePropertyName("code");

        RuleFor(request => request.GenerationType)
            .Must(value => IsType(value, VoucherGenerationTypes.None))
            .When(request =>
                IsType(request.PublishType, VoucherPublishTypes.Public) &&
                IsDefinedGenerationType(request.GenerationType))
            .WithErrorCode("VOUCHER_PUBLIC_GENERATION_TYPE_INVALID")
            .OverridePropertyName("generationType");

        RuleFor(request => request.GenerationType)
            .Must(value => !IsType(value, VoucherGenerationTypes.None))
            .When(request =>
                IsType(request.PublishType, VoucherPublishTypes.Private) &&
                IsDefinedGenerationType(request.GenerationType))
            .WithErrorCode("VOUCHER_PRIVATE_GENERATION_TYPE_INVALID")
            .OverridePropertyName("generationType");

        RuleFor(request => request.RewardValue)
            .Empty()
            .When(request => IsType(request.RewardType, VoucherRewardTypes.Gift))
            .WithErrorCode("VOUCHER_REWARD_VALUE_MUST_BE_EMPTY")
            .OverridePropertyName("rewardValue");

        RuleFor(request => request.RewardValue)
            .NotNull()
            .When(request => IsType(request.RewardType, VoucherRewardTypes.Fixed) || IsType(request.RewardType, VoucherRewardTypes.Percent))
            .WithErrorCode("VOUCHER_REWARD_VALUE_REQUIRED")
            .OverridePropertyName("rewardValue");

        RuleFor(request => request.RewardValue)
            .GreaterThan(0)
            .When(request => IsType(request.RewardType, VoucherRewardTypes.Fixed) && request.RewardValue.HasValue)
            .WithErrorCode("VOUCHER_FIXED_REWARD_VALUE_INVALID")
            .OverridePropertyName("rewardValue");

        RuleFor(request => request.RewardValue)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .When(request => IsType(request.RewardType, VoucherRewardTypes.Percent) && request.RewardValue.HasValue)
            .WithErrorCode("VOUCHER_PERCENT_REWARD_VALUE_INVALID")
            .OverridePropertyName("rewardValue");

        RuleFor(request => request.ValidFrom)
            .NotNull()
            .When(request => IsType(request.ValidityType, VoucherValidityTypes.Fixed))
            .WithErrorCode("VOUCHER_FIXED_VALID_FROM_REQUIRED")
            .OverridePropertyName("validFrom");

        RuleFor(request => request.ValidTo)
            .NotNull()
            .When(request => IsType(request.ValidityType, VoucherValidityTypes.Fixed))
            .WithErrorCode("VOUCHER_FIXED_VALID_TO_REQUIRED")
            .OverridePropertyName("validTo");

        RuleFor(request => request.ValidTo)
            .GreaterThan(request => request.ValidFrom)
            .When(request =>
                IsType(request.ValidityType, VoucherValidityTypes.Fixed) &&
                request.ValidFrom.HasValue &&
                request.ValidTo.HasValue)
            .WithErrorCode("VOUCHER_VALIDITY_RANGE_INVALID")
            .OverridePropertyName("validTo");

        RuleFor(request => request.ValidFrom)
            .NotNull()
            .When(request => IsType(request.ValidityType, VoucherValidityTypes.Dynamic))
            .WithErrorCode("VOUCHER_DYNAMIC_VALID_FROM_REQUIRED")
            .OverridePropertyName("validFrom");

        RuleFor(request => request.DurationDay)
            .NotNull()
            .When(request => IsType(request.ValidityType, VoucherValidityTypes.Dynamic))
            .WithErrorCode("VOUCHER_DYNAMIC_DURATION_DAY_REQUIRED")
            .OverridePropertyName("durationDay");
    }

    private static bool IsType(string? value, string expected)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Trim().Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDefinedGenerationType(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && VoucherGenerationTypes.IsDefined(value);
    }
}
