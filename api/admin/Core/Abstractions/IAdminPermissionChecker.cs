namespace Core.Abstractions;

public interface IAdminPermissionChecker
{
    Task<bool> HasPermissionAsync(
        Guid adminId,
        string permissionCode,
        CancellationToken ct = default);
}
