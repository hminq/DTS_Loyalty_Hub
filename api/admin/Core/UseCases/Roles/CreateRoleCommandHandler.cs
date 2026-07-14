using Core.Abstractions;
using Core.Entities;
using Core.Exceptions;
using Core.UseCases.Roles.Commands;
using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;

    public CreateRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
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

        var createdRole = await _roleRepository.CreateAsync(role, ct);

        return new RoleResult(
            createdRole.RoleId,
            createdRole.Name,
            createdRole.PermissionIds,
            createdRole.CreatedAt);
    }
}
