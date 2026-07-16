using Core.UseCases.AdminUsers.Results;

namespace Core.Abstractions;

public interface IRoleReader
{
    Task<AdminUserRoleResult?> GetDetailByIdAsync(
        Guid roleId,
        CancellationToken ct = default);
}
