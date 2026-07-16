using Core.UseCases.Notifications.Results;
using MediatR;
using System;

namespace Core.UseCases.Notifications.Commands;

public record CreateNotificationTemplateCommand(
    Guid NotificationEventTypeId,
    string Channel,
    string Language,
    string Name,
    string TitleTemplate,
    string BodyTemplate,
    [property: System.Text.Json.Serialization.JsonIgnore] Guid ActorUserId) : IRequest<NotificationTemplateResult>;
