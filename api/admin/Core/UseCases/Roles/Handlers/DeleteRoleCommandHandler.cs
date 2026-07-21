using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Roles.Commands;
using MediatR;
using System.Text.Json;
using Core.UseCases.AuditLogs;
using Core.Entities.Constants;

namespace Core.UseCases.Roles.Handlers;

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public DeleteRoleCommandHandler(IRoleRepository roleRepository, IAuditLogWriter auditLogWriter)
    {
        _roleRepository = roleRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        if (request.RoleId == Guid.Empty)
        {
            throw new DomainException(
                "ROLE_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId, ct);

        if (role is null)
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        if (await _roleRepository.HasAssignedAdminsAsync(request.RoleId, ct))
        {
            throw new DomainException(
                "ROLE_HAS_ASSIGNED_ADMINS",
                DomainErrorType.Conflict);
        }

        await _roleRepository.DeleteAsync(request.RoleId, ct);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId, AuditActions.Delete, AuditEntityTypes.Role, request.RoleId,
            JsonSerializer.Serialize(new { roleId = role.RoleId, name = role.Name, permissionIds = role.PermissionIds }), null, null));
    }
}
