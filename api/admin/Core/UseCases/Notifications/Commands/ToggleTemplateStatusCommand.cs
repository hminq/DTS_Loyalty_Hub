using Core.Abstractions;
using Core.UseCases.Notifications.Results;
using MediatR;
using System;

namespace Core.UseCases.Notifications.Commands;

public sealed record ToggleTemplateStatusCommand(Guid TemplateId, Guid ActorUserId) : IRequest<NotificationTemplateResult>, ITransactionalRequest;
