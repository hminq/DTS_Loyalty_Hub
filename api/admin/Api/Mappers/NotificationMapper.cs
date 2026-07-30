using System;
using Api.Dtos.Requests.Notifications;
using Core.UseCases.Notifications.Commands;

namespace Api.Mappers;

public static class NotificationMapper
{
    public static CreateNotificationTemplateCommand ToCommand(
        this CreateNotificationTemplateRequestDto request,
        Guid actorUserId)
    {
        return new CreateNotificationTemplateCommand(
            request.NotificationCode,
            request.Channel,
            request.Language,
            request.Name,
            request.TitleTemplate,
            request.BodyTemplate,
            request.Variables,
            actorUserId);
    }

    public static UpdateNotificationTemplateCommand ToCommand(
        this UpdateNotificationTemplateRequestDto request,
        Guid templateId,
        Guid actorUserId)
    {
        return new UpdateNotificationTemplateCommand(
            templateId,
            request.NotificationCode,
            request.Channel,
            request.Language,
            request.Name,
            request.TitleTemplate,
            request.BodyTemplate,
            request.Variables,
            request.IsActive,
            actorUserId);
    }
}
