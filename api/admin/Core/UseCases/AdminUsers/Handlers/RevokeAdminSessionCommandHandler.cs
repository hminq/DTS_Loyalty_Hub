using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Commands;
using MediatR;
using Core.UseCases.AuditLogs;
using Core.Entities.Constants;

namespace Core.UseCases.AdminUsers.Handlers;

public sealed class RevokeAdminSessionCommandHandler : IRequestHandler<RevokeAdminSessionCommand>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IAdminSessionRepository _adminSessionRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public RevokeAdminSessionCommandHandler(
        IAdminUserRepository adminUserRepository,
        IAdminSessionRepository adminSessionRepository,
        IAuditLogWriter auditLogWriter)
    {
        _adminUserRepository = adminUserRepository;
        _adminSessionRepository = adminSessionRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task Handle(RevokeAdminSessionCommand request, CancellationToken ct)
    {
        if (request.AdminId == Guid.Empty)
        {
            throw new DomainException(
                "ADMIN_ID_REQUIRED",
                "Admin id is required.",
                DomainErrorType.Validation);
        }

        var existingAdmin = await _adminUserRepository.GetByIdAsync(request.AdminId, ct);

        if (existingAdmin is null)
        {
            throw new DomainException(
                "ADMIN_USER_NOT_FOUND",
                "Admin user does not exist.",
                DomainErrorType.NotFound);
        }

        await _adminSessionRepository.RevokeActiveSessionsAsync(request.AdminId, ct);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId, "REVOKE_SESSION", AuditEntityTypes.Admin, request.AdminId, null, null, null));
    }
}
