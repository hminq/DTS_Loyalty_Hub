using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Roles.Queries;
using Core.UseCases.Roles.Results;
using MediatR;

namespace Core.UseCases.Roles.Handlers;

public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailResult>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleDetailResult> Handle(GetRoleByIdQuery request, CancellationToken ct)
    {
        if (request.RoleId == Guid.Empty)
        {
            throw new DomainException(
                "ROLE_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        var role = await _roleRepository.GetDetailByIdAsync(request.RoleId, ct);

        if (role is null)
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        return role;
    }
}
