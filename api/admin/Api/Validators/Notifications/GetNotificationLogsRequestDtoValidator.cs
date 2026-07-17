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
            .WithMessage("Page must be greater than or equal to 1.")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .WithMessage("Page size must be between 1 and 100.")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.EventTypeCode)
            .MaximumLength(50)
            .WithErrorCode("EVENT_TYPE_CODE_TOO_LONG")
            .WithMessage("Event type code cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.EventTypeCode))
            .OverridePropertyName("eventTypeCode");
    }
}
