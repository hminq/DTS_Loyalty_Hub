using Core.Abstractions;
using Core.UseCases.Permissions.Results;
using Persistence.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public PermissionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlySet<Guid>> GetExistingIdsAsync(
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken ct = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .Where(permission => permissionIds.Contains(permission.PermissionId))
            .Select(permission => permission.PermissionId)
            .ToHashSetAsync(ct);
    }

    public async Task<IReadOnlyCollection<PermissionGroupResult>> GetPermissionsAsync(
        CancellationToken ct = default)
    {
        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.GroupSortOrder)
            .ThenBy(permission => permission.GroupCode)
            .ThenBy(permission => permission.ActionSortOrder)
            .ThenBy(permission => permission.Code)
            .Select(permission => new
            {
                permission.PermissionId,
                permission.Code,
                permission.Name,
                permission.GroupCode,
                permission.GroupName,
                permission.ActionCode,
                permission.ActionName,
                permission.GroupSortOrder,
                permission.ActionSortOrder
            })
            .ToListAsync(ct);

        return permissions
            .GroupBy(permission => new
            {
                permission.GroupCode,
                permission.GroupName,
                permission.GroupSortOrder
            })
            .Select(group => new PermissionGroupResult(
                group.Key.GroupCode,
                group.Key.GroupName,
                group.Key.GroupSortOrder,
                group
                    .Select(permission => new PermissionResult(
                        permission.PermissionId,
                        permission.Code,
                        permission.Name,
                        permission.ActionCode,
                        permission.ActionName,
                        permission.ActionSortOrder))
                    .ToArray()))
            .ToArray();
    }
}
