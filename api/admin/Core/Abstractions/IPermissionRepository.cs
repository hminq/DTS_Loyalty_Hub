using Core.UseCases.Permissions.Results;

namespace Core.Abstractions;

public interface IPermissionRepository
{
    Task<IReadOnlyCollection<PermissionGroupResult>> GetPermissionsAsync(
        CancellationToken ct = default);
}
