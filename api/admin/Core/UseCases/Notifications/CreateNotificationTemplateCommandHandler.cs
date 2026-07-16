using System.Text.Json;
using Core.Abstractions;
using Core.Entities;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Notifications.Commands;
using Core.UseCases.Notifications.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Core.UseCases.Notifications;

public sealed class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, NotificationTemplateResult>
{
    private const string AuditLogEntityType = "NotificationTemplate";
    private const string AuditLogCreateAction = "CREATE";

    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationEventTypeRepository _eventTypeRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public CreateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        INotificationEventTypeRepository eventTypeRepository,
        IAuditLogRepository auditLogRepository)
    {
        _templateRepository = templateRepository;
        _eventTypeRepository = eventTypeRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<NotificationTemplateResult> Handle(CreateNotificationTemplateCommand request, CancellationToken ct)
    {
        var eventTypeExists = await _eventTypeRepository.ExistsAsync(request.NotificationEventTypeId, ct);
        if (!eventTypeExists)
        {
            throw new DomainException(
                "EVENT_TYPE_NOT_FOUND",
                "Notification event type does not exist.",
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

        var createdTemplate = await _templateRepository.CreateAsync(template, ct);

        await _auditLogRepository.CreateAsync(
            new AuditLogEntry(
                request.ActorUserId,
                AuditLogCreateAction,
                AuditLogEntityType,
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
                null),
            ct);

        var result = await _templateRepository.GetByIdAsync(createdTemplate.TemplateId, ct);
        if (result == null)
        {
            throw new InvalidOperationException("Failed to retrieve created template.");
        }
        
        return result;
    }
}
