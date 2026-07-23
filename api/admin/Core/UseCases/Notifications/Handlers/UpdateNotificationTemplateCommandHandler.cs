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
    private readonly INotificationEventTypeRepository _eventTypeRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public UpdateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        INotificationEventTypeRepository eventTypeRepository,
        IAuditLogWriter auditLogWriter)
    {
        _templateRepository = templateRepository;
        _eventTypeRepository = eventTypeRepository;
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
        
        var eventType = await _eventTypeRepository.GetByIdAsync(request.NotificationEventTypeId, ct);
        if (eventType == null)
        {
            throw new DomainException(
                "EVENT_TYPE_NOT_FOUND",
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

        if (template.IsActive)
        {
            await _templateRepository.DeactivateOtherTemplatesAsync(
                template.TemplateId, 
                template.NotificationEventTypeId, 
                template.Channel, 
                template.Language, 
                ct);
        }

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Update,
            AuditEntityTypes.NotificationTemplate,
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
            null));

        return new NotificationTemplateResult(
            template.TemplateId,
            template.NotificationEventTypeId,
            eventType.EventTypeCode,
            eventType.DisplayName,
            template.Channel,
            template.Language,
            template.Name,
            template.TitleTemplate,
            template.BodyTemplate,
            template.IsActive,
            template.CreatedBy,
            template.CreatedAt,
            template.UpdatedAt);
    }
}
