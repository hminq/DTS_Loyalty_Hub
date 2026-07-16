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
            .WithMessage("Voucher code must not exceed 200 characters.")
            .OverridePropertyName("code");

        RuleFor(request => request.Name)
            .NotEmpty()
            .WithErrorCode("VOUCHER_DEFINITION_NAME_REQUIRED")
            .WithMessage("Voucher definition name is required.")
            .MaximumLength(200)
            .WithErrorCode("VOUCHER_DEFINITION_NAME_TOO_LONG")
            .WithMessage("Voucher definition name must not exceed 200 characters.")
            .OverridePropertyName("name");

        RuleFor(request => request.RewardType)
            .NotEmpty()
            .WithErrorCode("VOUCHER_REWARD_TYPE_REQUIRED")
            .WithMessage("Voucher reward type is required.")
            .Must(VoucherRewardTypes.IsDefined)
            .WithErrorCode("VOUCHER_REWARD_TYPE_INVALID")
            .WithMessage("Voucher reward type is invalid.")
            .OverridePropertyName("rewardType");

        RuleFor(request => request.ValidityType)
            .NotEmpty()
            .WithErrorCode("VOUCHER_VALIDITY_TYPE_REQUIRED")
            .WithMessage("Voucher validity type is required.")
            .Must(VoucherValidityTypes.IsDefined)
            .WithErrorCode("VOUCHER_VALIDITY_TYPE_INVALID")
            .WithMessage("Voucher validity type is invalid.")
            .OverridePropertyName("validityType");

        RuleFor(request => request.GenerationType)
            .NotEmpty()
            .WithErrorCode("VOUCHER_GENERATION_TYPE_REQUIRED")
            .WithMessage("Voucher generation type is required.")
            .Must(VoucherGenerationTypes.IsDefined)
            .WithErrorCode("VOUCHER_GENERATION_TYPE_INVALID")
            .WithMessage("Voucher generation type is invalid.")
            .OverridePropertyName("generationType");

        RuleFor(request => request.PublishType)
            .NotEmpty()
            .WithErrorCode("VOUCHER_PUBLISH_TYPE_REQUIRED")
            .WithMessage("Voucher publish type is required.")
            .Must(VoucherPublishTypes.IsDefined)
            .WithErrorCode("VOUCHER_PUBLISH_TYPE_INVALID")
            .WithMessage("Voucher publish type is invalid.")
            .OverridePropertyName("publishType");

        RuleFor(request => request.TotalStock)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("VOUCHER_TOTAL_STOCK_INVALID")
            .WithMessage("Total stock cannot be negative.")
            .OverridePropertyName("totalStock");

        RuleFor(request => request.DurationDay)
            .GreaterThan(0)
            .When(request => request.DurationDay.HasValue)
            .WithErrorCode("VOUCHER_DURATION_DAY_INVALID")
            .WithMessage("Duration day must be greater than zero.")
            .OverridePropertyName("durationDay");

        RuleFor(request => request.Code)
            .NotEmpty()
            .When(request => IsType(request.PublishType, VoucherPublishTypes.Public))
            .WithErrorCode("VOUCHER_CODE_REQUIRED")
            .WithMessage("Voucher code is required for public vouchers.")
            .OverridePropertyName("code");

        RuleFor(request => request.Code)
            .Empty()
            .When(request => IsType(request.PublishType, VoucherPublishTypes.Private))
            .WithErrorCode("VOUCHER_CODE_MUST_BE_EMPTY")
            .WithMessage("Voucher code must be empty for private vouchers.")
            .OverridePropertyName("code");

        RuleFor(request => request.GenerationType)
            .Must(value => IsType(value, VoucherGenerationTypes.None))
            .When(request =>
                IsType(request.PublishType, VoucherPublishTypes.Public) &&
                IsDefinedGenerationType(request.GenerationType))
            .WithErrorCode("VOUCHER_PUBLIC_GENERATION_TYPE_INVALID")
            .WithMessage("Public vouchers must use NONE generation type.")
            .OverridePropertyName("generationType");

        RuleFor(request => request.GenerationType)
            .Must(value => !IsType(value, VoucherGenerationTypes.None))
            .When(request =>
                IsType(request.PublishType, VoucherPublishTypes.Private) &&
                IsDefinedGenerationType(request.GenerationType))
            .WithErrorCode("VOUCHER_PRIVATE_GENERATION_TYPE_INVALID")
            .WithMessage("Private vouchers cannot use NONE generation type.")
            .OverridePropertyName("generationType");

        RuleFor(request => request.RewardValue)
            .Empty()
            .When(request => IsType(request.RewardType, VoucherRewardTypes.Gift))
            .WithErrorCode("VOUCHER_REWARD_VALUE_MUST_BE_EMPTY")
            .WithMessage("Reward value must be empty for gift vouchers.")
            .OverridePropertyName("rewardValue");

        RuleFor(request => request.ValidFrom)
            .NotNull()
            .When(request => IsType(request.ValidityType, VoucherValidityTypes.Fixed))
            .WithErrorCode("VOUCHER_FIXED_VALIDITY_REQUIRED")
            .WithMessage("Valid from is required for fixed validity.")
            .OverridePropertyName("validFrom");

        RuleFor(request => request.ValidTo)
            .NotNull()
            .When(request => IsType(request.ValidityType, VoucherValidityTypes.Fixed))
            .WithErrorCode("VOUCHER_FIXED_VALIDITY_REQUIRED")
            .WithMessage("Valid to is required for fixed validity.")
            .OverridePropertyName("validTo");

        RuleFor(request => request.ValidTo)
            .GreaterThan(request => request.ValidFrom)
            .When(request =>
                IsType(request.ValidityType, VoucherValidityTypes.Fixed) &&
                request.ValidFrom.HasValue &&
                request.ValidTo.HasValue)
            .WithErrorCode("VOUCHER_VALIDITY_RANGE_INVALID")
            .WithMessage("Valid from must be earlier than valid to.")
            .OverridePropertyName("validTo");

        RuleFor(request => request.ValidFrom)
            .NotNull()
            .When(request => IsType(request.ValidityType, VoucherValidityTypes.Dynamic))
            .WithErrorCode("VOUCHER_DYNAMIC_VALIDITY_REQUIRED")
            .WithMessage("Valid from is required for dynamic validity.")
            .OverridePropertyName("validFrom");

        RuleFor(request => request.DurationDay)
            .NotNull()
            .When(request => IsType(request.ValidityType, VoucherValidityTypes.Dynamic))
            .WithErrorCode("VOUCHER_DYNAMIC_VALIDITY_REQUIRED")
            .WithMessage("Duration day is required for dynamic validity.")
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
