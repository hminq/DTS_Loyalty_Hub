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
    private readonly INotificationEventTypeRepository _eventTypeRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public CreateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        INotificationEventTypeRepository eventTypeRepository,
        IAuditLogWriter auditLogWriter)
    {
        _templateRepository = templateRepository;
        _eventTypeRepository = eventTypeRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<NotificationTemplateResult> Handle(CreateNotificationTemplateCommand request, CancellationToken ct)
    {
        var eventType = await _eventTypeRepository.GetByIdAsync(request.NotificationEventTypeId, ct);
        if (eventType == null)
        {
            throw new DomainException(
                "EVENT_TYPE_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        var template = NotificationTemplate.Create(
            request.NotificationEventTypeId,
            request.Channel,
            request.Language,
            request.Name,
            request.TitleTemplate,
            request.BodyTemplate,
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
                notificationEventTypeId = createdTemplate.NotificationEventTypeId,
                name = createdTemplate.Name,
                channel = createdTemplate.Channel,
                language = createdTemplate.Language,
                isActive = createdTemplate.IsActive
            }),
            null));

        return new NotificationTemplateResult(
            createdTemplate.TemplateId,
            createdTemplate.NotificationEventTypeId,
            eventType.EventTypeCode,
            eventType.DisplayName,
            createdTemplate.Channel,
            createdTemplate.Language,
            createdTemplate.Name,
            createdTemplate.TitleTemplate,
            createdTemplate.BodyTemplate,
            createdTemplate.IsActive,
            createdTemplate.CreatedBy,
            createdTemplate.CreatedAt,
            createdTemplate.UpdatedAt);
    }
}
