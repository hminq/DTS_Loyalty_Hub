using Core.Abstractions;
using Core.UseCases.Permissions.Queries;
using Core.UseCases.Permissions.Results;
using MediatR;

namespace Core.UseCases.Permissions.Handlers;

public sealed class GetPermissionsQueryHandler
    : IRequestHandler<GetPermissionsQuery, IReadOnlyCollection<PermissionGroupResult>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public Task<IReadOnlyCollection<PermissionGroupResult>> Handle(
        GetPermissionsQuery request,
        CancellationToken ct)
    {
        return _permissionRepository.GetPermissionsAsync(ct);
    }
}
