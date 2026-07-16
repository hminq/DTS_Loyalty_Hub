using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Roles.Commands;
using Core.UseCases.Roles.Results;
using MediatR;
using System.Text.Json;
using Core.UseCases.AuditLogs;
using Core.Entities.Constants;

namespace Core.UseCases.Roles.Handlers;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IAuditLogWriter auditLogWriter)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<RoleResult> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        if (request.RoleId == Guid.Empty)
        {
            throw new DomainException(
                "ROLE_ID_REQUIRED",
                "Role id is required.",
                DomainErrorType.Validation);
        }

        var permissionIds = request.PermissionIds
            .Distinct()
            .ToArray();

        if (permissionIds.Length != request.PermissionIds.Count)
        {
            throw new DomainException(
                "ROLE_PERMISSION_DUPLICATED",
                "Role permission ids must be unique.",
                DomainErrorType.Validation);
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId, ct);

        if (role is null)
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                "Role does not exist.",
                DomainErrorType.NotFound);
        }

        var oldValue = JsonSerializer.Serialize(new { name = role.Name, permissionIds = role.PermissionIds });
        role.Rename(request.Name);
        role.ReplacePermissions(permissionIds);

        if (await _roleRepository.ExistsByNameExceptAsync(role.Name, role.RoleId, ct))
        {
            throw new DomainException(
                "ROLE_NAME_ALREADY_EXISTS",
                "Role name already exists.",
                DomainErrorType.Conflict);
        }

        var existingPermissionIds = await _permissionRepository.GetExistingIdsAsync(permissionIds, ct);
        var missingPermissionIds = permissionIds
            .Where(permissionId => !existingPermissionIds.Contains(permissionId))
            .ToArray();

        if (missingPermissionIds.Length > 0)
        {
            throw new DomainException(
                "ROLE_PERMISSION_NOT_FOUND",
                "One or more permissions do not exist.",
                DomainErrorType.Validation);
        }

        var updatedRole = await _roleRepository.UpdateAsync(role, ct);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId, "UPDATE", AuditEntityTypes.Role, updatedRole.RoleId, oldValue,
            JsonSerializer.Serialize(new { name = updatedRole.Name, permissionIds = updatedRole.PermissionIds }), null));

        return new RoleResult(
            updatedRole.RoleId,
            updatedRole.Name,
            updatedRole.PermissionIds,
            updatedRole.CreatedAt);
    }
}
