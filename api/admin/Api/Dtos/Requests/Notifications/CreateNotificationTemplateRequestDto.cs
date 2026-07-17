using System;

namespace Api.Dtos.Requests.Notifications;

public sealed record CreateNotificationTemplateRequestDto(
    Guid NotificationEventTypeId,
    string Channel,
    string Language,
    string Name,
    string TitleTemplate,
    string BodyTemplate);
