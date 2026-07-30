using System.Text.Json;
using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Notifications.Commands;
using Core.UseCases.Notifications.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Core.UseCases.Notifications.Handlers;

public sealed class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, NotificationTemplateResult>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public CreateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        IAuditLogWriter auditLogWriter)
    {
        _templateRepository = templateRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<NotificationTemplateResult> Handle(CreateNotificationTemplateCommand request, CancellationToken ct)
    {
        var template = NotificationTemplate.Create(
            request.NotificationCode,
            request.Channel,
            request.Language,
            request.Name,
            request.TitleTemplate,
            request.BodyTemplate,
            request.Variables,
            request.ActorUserId);

        var createdTemplate = _templateRepository.Add(template);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Create,
            AuditEntityTypes.NotificationTemplate,
            createdTemplate.TemplateId,
            null,
            JsonSerializer.Serialize(new
            {
                templateId = createdTemplate.TemplateId,
                notificationCode = createdTemplate.NotificationCode,
                name = createdTemplate.Name,
                channel = createdTemplate.Channel,
                language = createdTemplate.Language,
                isActive = createdTemplate.IsActive
            }),
            null));

        return new NotificationTemplateResult(
            createdTemplate.TemplateId,
            createdTemplate.NotificationCode,
            createdTemplate.Channel,
            createdTemplate.Language,
            createdTemplate.Name,
            createdTemplate.TitleTemplate,
            createdTemplate.BodyTemplate,
            createdTemplate.Variables,
            createdTemplate.IsActive,
            createdTemplate.CreatedBy,
            createdTemplate.CreatedAt,
            createdTemplate.UpdatedAt);
    }
}
