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

public sealed class ToggleTemplateStatusCommandHandler : IRequestHandler<ToggleTemplateStatusCommand, NotificationTemplateResult>
{
    private const string AuditLogEntityType = "NotificationTemplate";
    private const string AuditLogUpdateAction = "TOGGLE_STATUS";

    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public ToggleTemplateStatusCommandHandler(
        INotificationTemplateRepository templateRepository,
        IAuditLogRepository auditLogRepository)
    {
        _templateRepository = templateRepository;
        _auditLogRepository = auditLogRepository;
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

        var oldState = JsonSerializer.Serialize(new { isActive = template.IsActive });

        template.ToggleStatus();

        await _templateRepository.UpdateAsync(template, ct);

        await _auditLogRepository.CreateAsync(
            new AuditLogEntry(
                request.ActorUserId,
                AuditLogUpdateAction,
                AuditLogEntityType,
                template.TemplateId,
                oldState,
                JsonSerializer.Serialize(new { isActive = template.IsActive }),
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
