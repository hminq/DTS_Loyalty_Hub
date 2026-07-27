using Api.Dtos.Requests.Campaigns;
using Campaign.Contracts.Constants;
using FluentValidation;
using Messaging.Contracts.Events;

namespace Api.Validators.Campaigns;

public sealed class GetCampaignsRequestDtoValidator : AbstractValidator<GetCampaignsRequestDto>
{
    public GetCampaignsRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.Keyword)
            .MaximumLength(100)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.Keyword is not null)
            .OverridePropertyName("keyword");

        RuleFor(request => request.Status)
            .Must(CampaignStatuses.IsDefined!)
            .WithErrorCode("CAMPAIGN_STATUS_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.Status))
            .OverridePropertyName("status");

        RuleFor(request => request.EventType)
            .Must(eventType => string.Equals(
                eventType?.Trim(),
                EventTypeCodes.CustomerAccountRegistered,
                StringComparison.OrdinalIgnoreCase))
            .WithErrorCode("CAMPAIGN_EVENT_TYPE_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.EventType))
            .OverridePropertyName("eventType");
    }
}
