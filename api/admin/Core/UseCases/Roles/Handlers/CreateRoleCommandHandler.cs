using Core.Abstractions;
using Core.Entities;
using Core.Exceptions;
using Core.UseCases.Roles.Commands;
using Core.UseCases.Roles.Results;
using MediatR;
using System.Text.Json;
using Core.UseCases.AuditLogs;
using Core.Entities.Constants;

namespace Core.UseCases.Roles.Handlers;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IAuditLogWriter auditLogWriter)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<RoleResult> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var permissionIds = request.PermissionIds
            .Distinct()
            .ToArray();

        if (permissionIds.Length != request.PermissionIds.Count)
        {
            throw new DomainException(
                "ROLE_PERMISSION_DUPLICATED",
                "Role permission ids must be unique.",
                DomainErrorType.Conflict);
        }

        var role = Role.Create(request.Name);
        role.ReplacePermissions(permissionIds);

        if (await _roleRepository.ExistsByNameAsync(role.Name, ct))
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

        var createdRole = _roleRepository.Add(role);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId, "CREATE", AuditEntityTypes.Role, createdRole.RoleId, null,
            JsonSerializer.Serialize(new { roleId = createdRole.RoleId, name = createdRole.Name, permissionIds = createdRole.PermissionIds }), null));

        return new RoleResult(
            createdRole.RoleId,
            createdRole.Name,
            createdRole.PermissionIds,
            createdRole.CreatedAt);
    }
}
