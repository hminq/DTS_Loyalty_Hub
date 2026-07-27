using Api.Dtos.Requests.Campaigns;
using FluentValidation;

namespace Api.Validators.Campaigns;

public sealed class CampaignActionWriteRequestDtoValidator
    : AbstractValidator<CampaignActionWriteRequestDto>
{
    public CampaignActionWriteRequestDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.ActionType)
            .NotEmpty()
            .WithErrorCode("CAMPAIGN_ACTION_TYPE_INVALID")
            .OverridePropertyName("actionType");

        RuleFor(request => request.ActionConfig)
            .Must(config => config.ValueKind == System.Text.Json.JsonValueKind.Object)
            .WithErrorCode("CAMPAIGN_ACTION_CONFIG_INVALID")
            .OverridePropertyName("actionConfig");

        RuleFor(request => request.ExecuteOrder)
            .GreaterThan(0)
            .WithErrorCode("CAMPAIGN_ACTION_ORDER_INVALID")
            .OverridePropertyName("executeOrder");

        RuleFor(request => request.TotalCount)
            .GreaterThanOrEqualTo(0)
            .When(request => request.TotalCount.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("totalCount");

        RuleFor(request => request.SessionCount)
            .GreaterThanOrEqualTo(0)
            .When(request => request.SessionCount.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("sessionCount");

        RuleFor(request => request.SessionCount)
            .LessThanOrEqualTo(request => request.TotalCount)
            .When(request => request.TotalCount.HasValue && request.SessionCount.HasValue)
            .WithErrorCode("CAMPAIGN_LIMIT_INVALID")
            .OverridePropertyName("sessionCount");

    }
}
