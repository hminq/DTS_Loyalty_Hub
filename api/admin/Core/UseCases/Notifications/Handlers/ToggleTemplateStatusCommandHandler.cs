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

public sealed class ToggleTemplateStatusCommandHandler : IRequestHandler<ToggleTemplateStatusCommand, NotificationTemplateResult>
{
    private const string AuditLogUpdateAction = "TOGGLE_STATUS";

    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationEventTypeRepository _eventTypeRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public ToggleTemplateStatusCommandHandler(
        INotificationTemplateRepository templateRepository,
        INotificationEventTypeRepository eventTypeRepository,
        IAuditLogWriter auditLogWriter)
    {
        _templateRepository = templateRepository;
        _eventTypeRepository = eventTypeRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<NotificationTemplateResult> Handle(ToggleTemplateStatusCommand request, CancellationToken ct)
    {
        var template = await _templateRepository.GetEntityByIdAsync(request.TemplateId, ct);
        if (template == null)
        {
            throw new DomainException(
                "TEMPLATE_NOT_FOUND",
                "Notification template does not exist.",
                DomainErrorType.NotFound);
        }

        var eventType = await _eventTypeRepository.GetByIdAsync(template.NotificationEventTypeId, ct);
        if (eventType == null)
        {
            throw new DomainException(
                "EVENT_TYPE_NOT_FOUND",
                "Notification event type does not exist.",
                DomainErrorType.NotFound);
        }

        var oldState = JsonSerializer.Serialize(new { isActive = template.IsActive });

        template.ToggleStatus();

        await _templateRepository.UpdateAsync(template, ct);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditLogUpdateAction,
            AuditEntityTypes.NotificationTemplate,
            template.TemplateId,
            oldState,
            JsonSerializer.Serialize(new { isActive = template.IsActive }),
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
