using Core.UseCases.Notifications.Results;
using MediatR;
using System;

namespace Core.UseCases.Notifications.Queries;

public record GetNotificationTemplateByIdQuery(Guid TemplateId) : IRequest<NotificationTemplateResult>;
