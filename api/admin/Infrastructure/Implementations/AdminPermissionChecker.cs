using Core.Abstractions;
using Core.Entities.Constants;
using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

public sealed class AdminPermissionChecker : IAdminPermissionChecker
{
    private readonly LoyaltyHubDbContext _dbContext;

    public AdminPermissionChecker(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasPermissionAsync(
        Guid adminId,
        string permissionCode,
        CancellationToken ct = default)
    {
        var normalizedPermissionCode = permissionCode.Trim().ToLowerInvariant();

        return _dbContext.Admins
            .AsNoTracking()
            .AnyAsync(admin =>
                admin.AdminId == adminId &&
                admin.User.UserType == UserTypes.Admin &&
                admin.User.Status == UserStatus.Enable &&
                admin.Role.RolePermissions.Any(rolePermission =>
                    rolePermission.Permission.Code == normalizedPermissionCode),
                ct);
    }
}
