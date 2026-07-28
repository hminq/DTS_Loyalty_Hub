using Api.Dtos.Requests.Campaigns;
using Campaign.Contracts.Schedules;
using Core.Entities.Constants;
using FluentValidation;

namespace Api.Validators.Campaigns;

internal static class CampaignWriteValidationRules
{
    public static void Apply<TRequest>(AbstractValidator<TRequest> validator)
        where TRequest : ICampaignWriteRequest
    {
        validator.RuleLevelCascadeMode = CascadeMode.Stop;

        validator.RuleFor(request => request.CampaignName)
            .NotEmpty()
            .WithErrorCode("CAMPAIGN_NAME_REQUIRED")
            .MaximumLength(200)
            .WithErrorCode("CAMPAIGN_NAME_TOO_LONG")
            .OverridePropertyName("campaignName");

        validator.RuleFor(request => request.BannerImageUrl)
            .Must(key => key!.StartsWith(BannerUploadTypes.CampaignBannerPrefix, StringComparison.Ordinal))
            .When(request => !string.IsNullOrWhiteSpace(request.BannerImageUrl))
            .WithErrorCode("CAMPAIGN_BANNER_IMAGE_KEY_INVALID")
            .OverridePropertyName("bannerImageUrl");

        validator.RuleFor(request => request.EventType)
            .NotEmpty()
            .WithErrorCode("CAMPAIGN_EVENT_TYPE_INVALID")
            .OverridePropertyName("eventType");

        validator.RuleFor(request => request.Condition)
            .Must(condition => condition.ValueKind == System.Text.Json.JsonValueKind.Object)
            .WithErrorCode("CAMPAIGN_CONDITION_INVALID")
            .OverridePropertyName("condition");

        validator.RuleFor(request => request.StartDate)
            .NotEqual(default(DateTimeOffset))
            .WithErrorCode("CAMPAIGN_DATE_RANGE_INVALID")
            .OverridePropertyName("startDate");

        validator.RuleFor(request => request.EndDate)
            .NotEqual(default(DateTimeOffset))
            .WithErrorCode("CAMPAIGN_DATE_RANGE_INVALID")
            .GreaterThan(request => request.StartDate)
            .WithErrorCode("CAMPAIGN_DATE_RANGE_INVALID")
            .OverridePropertyName("endDate");

        validator.RuleFor(request => request.ScheduleCron)
            .NotEmpty()
            .WithErrorCode("CAMPAIGN_SCHEDULE_INVALID")
            .MaximumLength(100)
            .WithErrorCode("CAMPAIGN_SCHEDULE_INVALID")
            .Must(scheduleCron => CampaignScheduleCron.TryParse(scheduleCron, out _))
            .WithErrorCode("CAMPAIGN_SCHEDULE_INVALID")
            .OverridePropertyName("scheduleCron");

        validator.RuleFor(request => request.DurationHour)
            .GreaterThan(0)
            .WithErrorCode("CAMPAIGN_DURATION_INVALID")
            .OverridePropertyName("durationHour");

        validator.RuleFor(request => request.UserLimitTotal)
            .GreaterThanOrEqualTo(0)
            .When(request => request.UserLimitTotal.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("userLimitTotal");

        validator.RuleFor(request => request.UserLimitSession)
            .GreaterThanOrEqualTo(0)
            .When(request => request.UserLimitSession.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("userLimitSession");

        validator.RuleFor(request => request.UserLimitSession)
            .LessThanOrEqualTo(request => request.UserLimitTotal)
            .When(request => request.UserLimitTotal.HasValue && request.UserLimitSession.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("userLimitSession");
    }
}
