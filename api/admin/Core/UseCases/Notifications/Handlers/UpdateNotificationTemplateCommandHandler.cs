using System.Text.Json;
using Core.Abstractions;
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

public sealed class UpdateNotificationTemplateCommandHandler : IRequestHandler<UpdateNotificationTemplateCommand, NotificationTemplateResult>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public UpdateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        IAuditLogWriter auditLogWriter)
    {
        _templateRepository = templateRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<NotificationTemplateResult> Handle(UpdateNotificationTemplateCommand request, CancellationToken ct)
    {
        var template = await _templateRepository.GetEntityByIdAsync(request.TemplateId, ct);
        if (template == null)
        {
            throw new DomainException(
                "TEMPLATE_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        if (request.IsActive && await _templateRepository.HasActiveTemplateAsync(
                template.TemplateId,
                request.NotificationCode,
                request.Channel,
                request.Language,
                ct))
        {
            throw new DomainException(
                "NOTIFICATION_TEMPLATE_ALREADY_ACTIVE",
                DomainErrorType.Conflict);
        }
        
        var oldState = JsonSerializer.Serialize(new
        {
            notificationCode = template.NotificationCode,
            channel = template.Channel,
            language = template.Language,
            name = template.Name,
            titleTemplate = template.TitleTemplate,
            bodyTemplate = template.BodyTemplate,
            isActive = template.IsActive
        });

        template.Update(
            request.NotificationCode,
            request.Channel,
            request.Language,
            request.Name, 
            request.TitleTemplate, 
            request.BodyTemplate,
            request.Variables,
            request.IsActive);

        await _templateRepository.UpdateAsync(template, ct);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Update,
            AuditEntityTypes.NotificationTemplate,
            template.TemplateId,
            oldState,
            JsonSerializer.Serialize(new
            {
                notificationCode = template.NotificationCode,
                channel = template.Channel,
                language = template.Language,
                name = template.Name,
                titleTemplate = template.TitleTemplate,
                bodyTemplate = template.BodyTemplate,
                isActive = template.IsActive
            }),
            null));

        return new NotificationTemplateResult(
            template.TemplateId,
            template.NotificationCode,
            template.Channel,
            template.Language,
            template.Name,
            template.TitleTemplate,
            template.BodyTemplate,
            template.Variables,
            template.IsActive,
            template.CreatedBy,
            template.CreatedAt,
            template.UpdatedAt);
    }
}
