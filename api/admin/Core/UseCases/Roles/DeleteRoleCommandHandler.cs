using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Roles.Commands;
using MediatR;

namespace Core.UseCases.Roles;

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository;

    public DeleteRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        if (request.RoleId == Guid.Empty)
        {
            throw new DomainException(
                "ROLE_ID_REQUIRED",
                "Role id is required.",
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

        if (await _roleRepository.HasAssignedAdminsAsync(request.RoleId, ct))
        {
            throw new DomainException(
                "ROLE_HAS_ASSIGNED_ADMINS",
                "Role cannot be deleted because it is assigned to one or more admins.",
                DomainErrorType.Conflict);
        }

        await _roleRepository.DeleteAsync(request.RoleId, ct);
    }
}
