using Core.Entities;
using Core.UseCases.Common;
using Core.UseCases.Roles.Results;

namespace Core.Abstractions;

public interface IRoleRepository
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    Task<bool> ExistsByNameExceptAsync(
        string name,
        Guid excludedRoleId,
        CancellationToken ct = default);

    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken ct = default);

    Task<PagedResult<RoleResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        CancellationToken ct = default);

    Task<bool> HasAssignedAdminsAsync(Guid roleId, CancellationToken ct = default);

    Role Add(Role role);

    Task<Role> UpdateAsync(Role role, CancellationToken ct = default);

    Task DeleteAsync(Guid roleId, CancellationToken ct = default);
}
