using Core.Entities;

namespace Api.Dtos.Requests.Notifications;

public sealed record CreateNotificationTemplateRequestDto(
    string NotificationCode,
    string Channel,
    string Language,
    string Name,
    string TitleTemplate,
    string BodyTemplate,
    IReadOnlyCollection<NotificationTemplateVariable> Variables);
