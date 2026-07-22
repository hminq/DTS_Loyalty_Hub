using Core.Abstractions;
using Core.UseCases.AdminUsers.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class RoleReader : IRoleReader
{
    private readonly LoyaltyHubDbContext _dbContext;

    public RoleReader(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdminUserRoleResult?> GetDetailByIdAsync(
        Guid roleId,
        CancellationToken ct = default)
    {
        return _dbContext.Roles
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
                        rolePermission.Permission.ActionCode,
                        rolePermission.Permission.ActionName,
                        rolePermission.Permission.GroupSortOrder,
                        rolePermission.Permission.ActionSortOrder))
                    .ToArray()))
            .SingleOrDefaultAsync(ct);
    }
}
