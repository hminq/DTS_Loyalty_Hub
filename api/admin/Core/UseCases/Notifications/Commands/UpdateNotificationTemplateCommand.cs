using Core.Abstractions;
using Core.UseCases.Notifications.Results;
using MediatR;
using System;

namespace Core.UseCases.Notifications.Commands;

public sealed record UpdateNotificationTemplateCommand(
    [property: System.Text.Json.Serialization.JsonIgnore] Guid TemplateId,
    Guid NotificationEventTypeId,
    string Channel,
    string Language,
    string Name,
    string TitleTemplate,
    string BodyTemplate,
    bool IsActive,
    [property: System.Text.Json.Serialization.JsonIgnore] Guid ActorUserId) : IRequest<NotificationTemplateResult>, ITransactionalRequest;
