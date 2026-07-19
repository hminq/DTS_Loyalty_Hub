using Api.Dtos.Requests.Notifications;
using FluentValidation;

namespace Api.Validators.Notifications;

public sealed class UpdateNotificationTemplateRequestDtoValidator : AbstractValidator<UpdateNotificationTemplateRequestDto>
{
    public UpdateNotificationTemplateRequestDtoValidator()
    {
        RuleFor(x => x.NotificationEventTypeId)
            .NotEmpty()
            .WithErrorCode("NOTIFICATION_EVENT_TYPE_ID_REQUIRED")
            .OverridePropertyName("notificationEventTypeId");

        RuleFor(x => x.Channel)
            .NotEmpty()
            .WithErrorCode("CHANNEL_REQUIRED")
            .MaximumLength(50)
            .WithErrorCode("CHANNEL_TOO_LONG")
            .OverridePropertyName("channel");

        RuleFor(x => x.Language)
            .NotEmpty()
            .WithErrorCode("LANGUAGE_REQUIRED")
            .MaximumLength(10)
            .WithErrorCode("LANGUAGE_TOO_LONG")
            .OverridePropertyName("language");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("NAME_REQUIRED")
            .MaximumLength(200)
            .WithErrorCode("NAME_TOO_LONG")
            .OverridePropertyName("name");

        RuleFor(x => x.TitleTemplate)
            .NotEmpty()
            .WithErrorCode("TITLE_TEMPLATE_REQUIRED")
            .MaximumLength(1000)
            .WithErrorCode("TITLE_TEMPLATE_TOO_LONG")
            .OverridePropertyName("titleTemplate");

        RuleFor(x => x.BodyTemplate)
            .NotEmpty()
            .WithErrorCode("BODY_TEMPLATE_REQUIRED")
            .MaximumLength(5000)
            .WithErrorCode("BODY_TEMPLATE_TOO_LONG")
            .OverridePropertyName("bodyTemplate");
    }
}
