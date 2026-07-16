using System.Text.Json;
using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Notifications.Commands;
using Core.UseCases.Notifications.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Core.UseCases.Notifications;

public sealed class UpdateNotificationTemplateCommandHandler : IRequestHandler<UpdateNotificationTemplateCommand, NotificationTemplateResult>
{
    private const string AuditLogEntityType = "NotificationTemplate";
    private const string AuditLogUpdateAction = "UPDATE";

    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public UpdateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        IAuditLogRepository auditLogRepository)
    {
        _templateRepository = templateRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<NotificationTemplateResult> Handle(UpdateNotificationTemplateCommand request, CancellationToken ct)
    {
        var template = await _templateRepository.GetEntityByIdAsync(request.TemplateId, ct);
        if (template == null)
        {
            throw new DomainException(
                "TEMPLATE_NOT_FOUND",
                "Notification template does not exist.",
                DomainErrorType.NotFound);
        }

        var oldState = JsonSerializer.Serialize(new
        {
            notificationEventTypeId = template.NotificationEventTypeId,
            channel = template.Channel,
            language = template.Language,
            name = template.Name,
            titleTemplate = template.TitleTemplate,
            bodyTemplate = template.BodyTemplate,
            isActive = template.IsActive
        });

        template.Update(
            request.NotificationEventTypeId,
            request.Channel,
            request.Language,
            request.Name, 
            request.TitleTemplate, 
            request.BodyTemplate,
            request.IsActive);

        await _templateRepository.UpdateAsync(template, ct);

        await _auditLogRepository.CreateAsync(
            new AuditLogEntry(
                request.ActorUserId,
                AuditLogUpdateAction,
                AuditLogEntityType,
                template.TemplateId,
                oldState,
                JsonSerializer.Serialize(new
                {
                    notificationEventTypeId = template.NotificationEventTypeId,
                    channel = template.Channel,
                    language = template.Language,
                    name = template.Name,
                    titleTemplate = template.TitleTemplate,
                    bodyTemplate = template.BodyTemplate,
                    isActive = template.IsActive
                }),
                null),
            ct);

        var result = await _templateRepository.GetByIdAsync(template.TemplateId, ct);
        if (result == null)
        {
             throw new InvalidOperationException("Failed to retrieve updated template.");
        }
        return result;
    }
}
