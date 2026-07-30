using Core.Abstractions;
using Core.UseCases.Notifications.Results;
using MediatR;
using System;
using Core.Entities;

namespace Core.UseCases.Notifications.Commands;

public sealed record CreateNotificationTemplateCommand(
    string NotificationCode,
    string Channel,
    string Language,
    string Name,
    string TitleTemplate,
    string BodyTemplate,
    IReadOnlyCollection<NotificationTemplateVariable> Variables,
    [property: System.Text.Json.Serialization.JsonIgnore] Guid ActorUserId) : IRequest<NotificationTemplateResult>, ITransactionalRequest;
