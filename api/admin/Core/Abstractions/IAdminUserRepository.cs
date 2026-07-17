using Core.UseCases.AdminUsers.Results;
using Core.UseCases.Common;

namespace Core.Abstractions;

public interface IAdminUserRepository
{
    Task<PagedResult<AdminUserResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? status,
        Guid? roleId,
        CancellationToken ct = default);

    Task<AdminUserResult?> GetByIdAsync(Guid adminId, CancellationToken ct = default);

}
