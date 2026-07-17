using Api.Dtos.Requests.Notifications;
using FluentValidation;

namespace Api.Validators.Notifications;

public sealed class CreateNotificationTemplateRequestDtoValidator : AbstractValidator<CreateNotificationTemplateRequestDto>
{
    public CreateNotificationTemplateRequestDtoValidator()
    {
        RuleFor(x => x.NotificationEventTypeId)
            .NotEmpty()
            .WithErrorCode("NOTIFICATION_EVENT_TYPE_ID_REQUIRED")
            .WithMessage("Notification event type ID is required.")
            .OverridePropertyName("notificationEventTypeId");

        RuleFor(x => x.Channel)
            .NotEmpty()
            .WithErrorCode("CHANNEL_REQUIRED")
            .WithMessage("Channel is required.")
            .MaximumLength(50)
            .WithErrorCode("CHANNEL_TOO_LONG")
            .WithMessage("Channel cannot exceed 50 characters.")
            .OverridePropertyName("channel");

        RuleFor(x => x.Language)
            .NotEmpty()
            .WithErrorCode("LANGUAGE_REQUIRED")
            .WithMessage("Language is required.")
            .MaximumLength(10)
            .WithErrorCode("LANGUAGE_TOO_LONG")
            .WithMessage("Language cannot exceed 10 characters.")
            .OverridePropertyName("language");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("NAME_REQUIRED")
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithErrorCode("NAME_TOO_LONG")
            .WithMessage("Name cannot exceed 200 characters.")
            .OverridePropertyName("name");

        RuleFor(x => x.TitleTemplate)
            .NotEmpty()
            .WithErrorCode("TITLE_TEMPLATE_REQUIRED")
            .WithMessage("Title template is required.")
            .OverridePropertyName("titleTemplate");

        RuleFor(x => x.BodyTemplate)
            .NotEmpty()
            .WithErrorCode("BODY_TEMPLATE_REQUIRED")
            .WithMessage("Body template is required.")
            .OverridePropertyName("bodyTemplate");
    }
}
