using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Roles.Commands;
using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;

    public UpdateRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
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

        role.Rename(request.Name);
        role.ReplacePermissions(permissionIds);

        if (await _roleRepository.ExistsByNameExceptAsync(role.Name, role.RoleId, ct))
        {
            throw new DomainException(
                "ROLE_NAME_ALREADY_EXISTS",
                "Role name already exists.",
                DomainErrorType.Conflict);
        }

        var existingPermissionIds = await _roleRepository.GetExistingPermissionIdsAsync(permissionIds, ct);
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

        return new RoleResult(
            updatedRole.RoleId,
            updatedRole.Name,
            updatedRole.PermissionIds,
            updatedRole.CreatedAt);
    }
}
