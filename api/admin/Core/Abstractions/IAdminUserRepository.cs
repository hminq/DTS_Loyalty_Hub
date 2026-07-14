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

    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken ct = default);

    Task<bool> EmailExistsExceptAsync(string email, Guid adminId, CancellationToken ct = default);

    Task<bool> PhoneNumberExistsExceptAsync(string phoneNumber, Guid adminId, CancellationToken ct = default);

    Task<bool> RoleExistsAsync(Guid roleId, CancellationToken ct = default);

    Task<AdminUserResult> CreateAsync(
        string username,
        string email,
        string passwordHash,
        string? fullName,
        string? phoneNumber,
        Guid roleId,
        CancellationToken ct = default);

    Task<AdminUserResult> UpdateAsync(
        Guid adminId,
        string email,
        string? fullName,
        string? phoneNumber,
        Guid roleId,
        CancellationToken ct = default);

    Task UpdateStatusAsync(
        Guid adminId,
        string status,
        CancellationToken ct = default);

    Task RevokeActiveSessionsAsync(Guid adminId, CancellationToken ct = default);
}
