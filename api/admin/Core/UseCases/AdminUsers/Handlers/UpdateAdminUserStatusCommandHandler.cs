using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using MediatR;
using System.Text.Json;
using Core.UseCases.AuditLogs;

namespace Core.UseCases.AdminUsers.Handlers;

public sealed class UpdateAdminUserStatusCommandHandler : IRequestHandler<UpdateAdminUserStatusCommand>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAdminSessionRepository _adminSessionRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public UpdateAdminUserStatusCommandHandler(
        IAdminUserRepository adminUserRepository,
        IUserRepository userRepository,
        IAdminSessionRepository adminSessionRepository,
        IAuditLogWriter auditLogWriter)
    {
        _adminUserRepository = adminUserRepository;
        _userRepository = userRepository;
        _adminSessionRepository = adminSessionRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task Handle(UpdateAdminUserStatusCommand request, CancellationToken ct)
    {
        if (request.AdminId == Guid.Empty)
        {
            throw new DomainException(
                "ADMIN_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        var normalizedStatus = request.Status.Trim().ToUpperInvariant();

        if (normalizedStatus is not UserStatus.Enable and not UserStatus.Disable)
        {
            throw new DomainException(
                "ADMIN_STATUS_INVALID",
                DomainErrorType.Validation);
        }

        var existingAdmin = await _adminUserRepository.GetByIdAsync(request.AdminId, ct);

        if (existingAdmin is null)
        {
            throw new DomainException(
                "ADMIN_USER_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        await _userRepository.UpdateAdminStatusAsync(request.AdminId, normalizedStatus, ct);

        // if status updated to Disable, also revoke the session
        if (normalizedStatus == UserStatus.Disable)
        {
            await _adminSessionRepository.RevokeActiveSessionsAsync(request.AdminId, ct);
        }

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId, AuditActions.UpdateStatus, AuditEntityTypes.Admin, request.AdminId,
            JsonSerializer.Serialize(new { status = existingAdmin.Status }),
            JsonSerializer.Serialize(new { status = normalizedStatus }), null));
    }
}
