using Core.Abstractions;
using Core.UseCases.Permissions.Queries;
using Core.UseCases.Permissions.Results;
using MediatR;

namespace Core.UseCases.Permissions;

public sealed class GetPermissionMatrixQueryHandler
    : IRequestHandler<GetPermissionMatrixQuery, IReadOnlyCollection<PermissionGroupResult>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionMatrixQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public Task<IReadOnlyCollection<PermissionGroupResult>> Handle(
        GetPermissionMatrixQuery request,
        CancellationToken ct)
    {
        return _permissionRepository.GetPermissionMatrixAsync(ct);
    }
}
