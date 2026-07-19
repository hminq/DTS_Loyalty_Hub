using Api.Dtos.Requests.Notifications;
using FluentValidation;

namespace Api.Validators.Notifications;

public sealed class GetNotificationLogsRequestDtoValidator : AbstractValidator<GetNotificationLogsRequestDto>
{
    public GetNotificationLogsRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.EventTypeCode)
            .MaximumLength(50)
            .WithErrorCode("EVENT_TYPE_CODE_TOO_LONG")
            .When(x => !string.IsNullOrEmpty(x.EventTypeCode))
            .OverridePropertyName("eventTypeCode");
    }
}
