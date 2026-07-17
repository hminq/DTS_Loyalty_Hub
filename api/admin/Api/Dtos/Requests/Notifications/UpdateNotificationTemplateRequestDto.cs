using System;

namespace Api.Dtos.Requests.Notifications;

public sealed record UpdateNotificationTemplateRequestDto(
    Guid NotificationEventTypeId,
    string Channel,
    string Language,
    string Name,
    string TitleTemplate,
    string BodyTemplate,
    bool IsActive);
