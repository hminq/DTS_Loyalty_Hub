using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Roles.Queries;
using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles;

public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleResult>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResult> Handle(GetRoleByIdQuery request, CancellationToken ct)
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

        return new RoleResult(
            role.RoleId,
            role.Name,
            role.PermissionIds,
            role.CreatedAt);
    }
}
