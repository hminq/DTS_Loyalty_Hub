using Api.Dtos.Requests.Campaigns;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.Campaigns;

public sealed class CampaignWriteRequestDtoValidator
    : AbstractValidator<CampaignWriteRequestDto>
{
    public CampaignWriteRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.CampaignName)
            .NotEmpty()
            .WithErrorCode("CAMPAIGN_NAME_REQUIRED")
            .MaximumLength(200)
            .WithErrorCode("CAMPAIGN_NAME_TOO_LONG")
            .OverridePropertyName("campaignName");

        RuleFor(request => request.BannerImageUrl)
            .Must(key => key!.StartsWith(BannerUploadTypes.CampaignBannerPrefix, StringComparison.Ordinal))
            .When(request => !string.IsNullOrWhiteSpace(request.BannerImageUrl))
            .WithErrorCode("CAMPAIGN_BANNER_IMAGE_KEY_INVALID")
            .OverridePropertyName("bannerImageUrl");

        RuleFor(request => request.EventType)
            .NotEmpty()
            .WithErrorCode("CAMPAIGN_EVENT_TYPE_INVALID")
            .OverridePropertyName("eventType");

        RuleFor(request => request.Condition)
            .Must(condition => condition.ValueKind == System.Text.Json.JsonValueKind.Object)
            .WithErrorCode("CAMPAIGN_CONDITION_INVALID")
            .OverridePropertyName("condition");

        RuleFor(request => request.StartDate)
            .NotEqual(default(DateTimeOffset))
            .WithErrorCode("CAMPAIGN_DATE_RANGE_INVALID")
            .OverridePropertyName("startDate");

        RuleFor(request => request.EndDate)
            .NotEqual(default(DateTimeOffset))
            .WithErrorCode("CAMPAIGN_DATE_RANGE_INVALID")
            .GreaterThan(request => request.StartDate)
            .WithErrorCode("CAMPAIGN_DATE_RANGE_INVALID")
            .OverridePropertyName("endDate");

        RuleFor(request => request.ScheduleCron)
            .NotEmpty()
            .WithErrorCode("CAMPAIGN_SCHEDULE_INVALID")
            .MaximumLength(100)
            .WithErrorCode("CAMPAIGN_SCHEDULE_INVALID")
            .OverridePropertyName("scheduleCron");

        RuleFor(request => request.DurationHour)
            .GreaterThan(0)
            .WithErrorCode("CAMPAIGN_DURATION_INVALID")
            .OverridePropertyName("durationHour");

        RuleFor(request => request.UserLimitTotal)
            .GreaterThanOrEqualTo(0)
            .When(request => request.UserLimitTotal.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("userLimitTotal");

        RuleFor(request => request.UserLimitSession)
            .GreaterThanOrEqualTo(0)
            .When(request => request.UserLimitSession.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("userLimitSession");

        RuleFor(request => request.UserLimitSession)
            .LessThanOrEqualTo(request => request.UserLimitTotal)
            .When(request => request.UserLimitTotal.HasValue && request.UserLimitSession.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("userLimitSession");
    }
}
