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
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public ToggleTemplateStatusCommandHandler(
        INotificationTemplateRepository templateRepository,
        IAuditLogWriter auditLogWriter)
    {
        _templateRepository = templateRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<NotificationTemplateResult> Handle(ToggleTemplateStatusCommand request, CancellationToken ct)
    {
        var template = await _templateRepository.GetEntityByIdAsync(request.TemplateId, ct);
        if (template == null)
        {
            throw new DomainException(
                "TEMPLATE_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        var oldState = JsonSerializer.Serialize(new { isActive = template.IsActive });

        template.ToggleStatus();

        await _templateRepository.UpdateAsync(template, ct);

        if (template.IsActive)
        {
            await _templateRepository.DeactivateOtherTemplatesAsync(
                template.TemplateId, 
                template.NotificationCode,
                template.Channel, 
                template.Language, 
                ct);
        }

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.ToggleStatus,
            AuditEntityTypes.NotificationTemplate,
            template.TemplateId,
            oldState,
            JsonSerializer.Serialize(new { isActive = template.IsActive }),
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
