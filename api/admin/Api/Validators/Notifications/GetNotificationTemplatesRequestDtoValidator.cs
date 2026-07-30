using Api.Dtos.Requests.Notifications;
using FluentValidation;

namespace Api.Validators.Notifications;

public sealed class GetNotificationTemplatesRequestDtoValidator : AbstractValidator<GetNotificationTemplatesRequestDto>
{
    public GetNotificationTemplatesRequestDtoValidator()
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
            .When(x => !string.IsNullOrEmpty(x.Keyword))
            .OverridePropertyName("keyword");

        RuleFor(request => request.NotificationCode)
            .MaximumLength(50)
            .WithErrorCode("NOTIFICATION_CODE_TOO_LONG")
            .When(x => !string.IsNullOrEmpty(x.NotificationCode))
            .OverridePropertyName("notificationCode");

        RuleFor(request => request.Channel)
            .MaximumLength(50)
            .WithErrorCode("CHANNEL_TOO_LONG")
            .When(x => !string.IsNullOrEmpty(x.Channel))
            .OverridePropertyName("channel");

        RuleFor(request => request.Language)
            .MaximumLength(10)
            .WithErrorCode("LANGUAGE_TOO_LONG")
            .When(x => !string.IsNullOrEmpty(x.Language))
            .OverridePropertyName("language");
    }
}
