using Core.UseCases.Permissions.Results;

namespace Core.Abstractions;

public interface IPermissionRepository
{
    Task<IReadOnlySet<Guid>> GetExistingIdsAsync(
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<PermissionGroupResult>> GetPermissionsAsync(
        CancellationToken ct = default);
}
