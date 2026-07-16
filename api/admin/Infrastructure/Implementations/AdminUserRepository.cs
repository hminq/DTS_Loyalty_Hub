using Core.Abstractions;
using Core.Entities.Constants;
using Core.UseCases.AdminUsers.Results;
using Core.UseCases.Common;
using Persistence.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

public sealed class AdminUserRepository : IAdminUserRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public AdminUserRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminUserResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? status,
        Guid? roleId,
        CancellationToken ct = default)
    {
        var query = _dbContext.Admins
            .AsNoTracking()
            .Where(admin => admin.User.UserType == UserTypes.Admin);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(admin =>
                EF.Functions.ILike(admin.User.Username, pattern) ||
                EF.Functions.ILike(admin.User.Email, pattern) ||
                (admin.User.FullName != null && EF.Functions.ILike(admin.User.FullName, pattern)) ||
                (admin.User.PhoneNumber != null && EF.Functions.ILike(admin.User.PhoneNumber, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToUpperInvariant();
            query = query.Where(admin => admin.User.Status == normalizedStatus);
        }

        if (roleId.HasValue)
        {
            query = query.Where(admin => admin.RoleId == roleId.Value);
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(admin => admin.CreatedAt)
            .ThenBy(admin => admin.User.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(admin => new AdminUserResult(
                admin.AdminId,
                admin.User.UserId,
                admin.User.Username,
                admin.User.Email,
                admin.User.FullName ?? string.Empty,
                admin.User.PhoneNumber,
                admin.Role.RoleId,
                admin.Role.Name,
                admin.User.Status,
                admin.CreatedAt))
            .ToArrayAsync(ct);

        return new PagedResult<AdminUserResult>(
            items,
            page,
            pageSize,
            totalItems);
    }

    public async Task<AdminUserResult?> GetByIdAsync(Guid adminId, CancellationToken ct = default)
    {
        var admin = await _dbContext.Admins
            .AsNoTracking()
            .Include(admin => admin.User)
            .Include(admin => admin.Role)
                .ThenInclude(role => role.RolePermissions)
                    .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(admin =>
                admin.AdminId == adminId &&
                admin.User.UserType == UserTypes.Admin,
                ct);

        if (admin is null)
        {
            return null;
        }

        var permissions = admin.Role.RolePermissions
            .OrderBy(rolePermission => rolePermission.Permission.GroupSortOrder)
            .ThenBy(rolePermission => rolePermission.Permission.ActionSortOrder)
            .ThenBy(rolePermission => rolePermission.Permission.Code)
            .Select(rolePermission => new AdminUserPermissionResult(
                rolePermission.Permission.PermissionId,
                rolePermission.Permission.Code,
                rolePermission.Permission.Name,
                rolePermission.Permission.GroupCode,
                rolePermission.Permission.GroupName,
                rolePermission.Permission.GroupSortOrder,
                rolePermission.Permission.ActionSortOrder))
            .ToArray();

        return new AdminUserResult(
            admin.AdminId,
            admin.User.UserId,
            admin.User.Username,
            admin.User.Email,
            admin.User.FullName ?? string.Empty,
            admin.User.PhoneNumber,
            admin.Role.RoleId,
            admin.Role.Name,
            admin.User.Status,
            admin.CreatedAt,
            new AdminUserRoleResult(
                admin.Role.RoleId,
                admin.Role.Name,
                permissions));
    }

    public async Task<AdminUserRoleResult?> GetRoleByIdAsync(
        Guid roleId,
        CancellationToken ct = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.RoleId == roleId)
            .Select(role => new AdminUserRoleResult(
                role.RoleId,
                role.Name,
                role.RolePermissions
                    .OrderBy(rolePermission => rolePermission.Permission.GroupSortOrder)
                    .ThenBy(rolePermission => rolePermission.Permission.ActionSortOrder)
                    .ThenBy(rolePermission => rolePermission.Permission.Code)
                    .Select(rolePermission => new AdminUserPermissionResult(
                        rolePermission.Permission.PermissionId,
                        rolePermission.Permission.Code,
                        rolePermission.Permission.Name,
                        rolePermission.Permission.GroupCode,
                        rolePermission.Permission.GroupName,
                        rolePermission.Permission.GroupSortOrder,
                        rolePermission.Permission.ActionSortOrder))
                    .ToArray()))
            .SingleOrDefaultAsync(ct);
    }

}
