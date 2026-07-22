using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.Roles.Results;
using Persistence.Models.Context;
using Microsoft.EntityFrameworkCore;
using DomainRole = Core.Entities.Role;
using PersistenceRole = Persistence.Models.Role;
using PersistenceRolePermission = Persistence.Models.RolePermission;

namespace Infrastructure.Implementations;

public sealed class RoleRepository : IRoleRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public RoleRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        var normalizedName = name.Trim();

        return _dbContext.Roles
            .AsNoTracking()
            .AnyAsync(role => role.Name == normalizedName, ct);
    }

    public Task<bool> ExistsByNameExceptAsync(
        string name,
        Guid excludedRoleId,
        CancellationToken ct = default)
    {
        var normalizedName = name.Trim();

        return _dbContext.Roles
            .AsNoTracking()
            .AnyAsync(role =>
                role.Name == normalizedName &&
                role.RoleId != excludedRoleId,
                ct);
    }

    public async Task<DomainRole?> GetByIdAsync(Guid roleId, CancellationToken ct = default)
    {
        var role = await _dbContext.Roles
            .AsNoTracking()
            .Include(role => role.RolePermissions)
            .FirstOrDefaultAsync(role => role.RoleId == roleId, ct);

        if (role is null)
        {
            return null;
        }

        return DomainRole.Restore(
            role.RoleId,
            role.Name,
            role.RolePermissions.Select(rolePermission => rolePermission.PermissionId),
            role.CreatedAt);
    }

    public Task<RoleDetailResult?> GetDetailByIdAsync(
        Guid roleId,
        CancellationToken ct = default)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.RoleId == roleId)
            .Select(role => new RoleDetailResult(
                role.RoleId,
                role.Name,
                role.RolePermissions
                    .OrderBy(rolePermission => rolePermission.Permission.GroupSortOrder)
                    .ThenBy(rolePermission => rolePermission.Permission.GroupCode)
                    .ThenBy(rolePermission => rolePermission.Permission.ActionSortOrder)
                    .ThenBy(rolePermission => rolePermission.Permission.Code)
                    .Select(rolePermission => new RolePermissionDetailResult(
                        rolePermission.Permission.PermissionId,
                        rolePermission.Permission.Code,
                        rolePermission.Permission.Name,
                        rolePermission.Permission.GroupCode,
                        rolePermission.Permission.GroupName,
                        rolePermission.Permission.ActionCode,
                        rolePermission.Permission.ActionName,
                        rolePermission.Permission.GroupSortOrder,
                        rolePermission.Permission.ActionSortOrder))
                    .ToArray(),
                role.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<RoleResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        CancellationToken ct = default)
    {
        var query = _dbContext.Roles
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(role => EF.Functions.ILike(role.Name, pattern));
        }

        var totalItems = await query.CountAsync(ct);

        var roles = await query
            .Include(role => role.RolePermissions)
            .OrderBy(role => role.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = roles
            .Select(role => new RoleResult(
                role.RoleId,
                role.Name,
                role.RolePermissions
                    .Select(rolePermission => rolePermission.PermissionId)
                    .ToArray(),
                role.CreatedAt))
            .ToArray();

        return new PagedResult<RoleResult>(
            items,
            page,
            pageSize,
            totalItems);
    }

    public async Task<IReadOnlyCollection<RoleOptionResult>> SearchOptionsAsync(
        string? keyword,
        int limit,
        CancellationToken ct = default)
    {
        var query = _dbContext.Roles
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(role => EF.Functions.ILike(role.Name, pattern));
        }

        return await query
            .OrderBy(role => role.Name.ToLower())
            .ThenBy(role => role.RoleId)
            .Select(role => new RoleOptionResult(
                role.RoleId,
                role.Name))
            .Take(limit)
            .ToArrayAsync(ct);
    }

    public Task<bool> HasAssignedAdminsAsync(Guid roleId, CancellationToken ct = default)
    {
        return _dbContext.Admins
            .AsNoTracking()
            .AnyAsync(admin => admin.RoleId == roleId, ct);
    }

    public DomainRole Add(DomainRole role)
    {
        var createdAt = role.CreatedAt;

        _dbContext.Roles.Add(new PersistenceRole
        {
            RoleId = role.RoleId,
            Name = role.Name,
            CreatedAt = createdAt
        });

        foreach (var permissionId in role.PermissionIds)
        {
            _dbContext.RolePermissions.Add(new PersistenceRolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                RoleId = role.RoleId,
                PermissionId = permissionId,
                CreatedAt = createdAt
            });
        }

        return role;
    }

    public async Task<DomainRole> UpdateAsync(DomainRole role, CancellationToken ct = default)
    {
        var persistedRole = await _dbContext.Roles
            .FirstOrDefaultAsync(persistedRole => persistedRole.RoleId == role.RoleId, ct);

        if (persistedRole is null)
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        persistedRole.Name = role.Name;

        var oldRolePermissions = await _dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == role.RoleId)
            .ToListAsync(ct);

        _dbContext.RolePermissions.RemoveRange(oldRolePermissions);

        foreach (var permissionId in role.PermissionIds)
        {
            _dbContext.RolePermissions.Add(new PersistenceRolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                RoleId = role.RoleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            });
        }

        return role;
    }

    public async Task DeleteAsync(Guid roleId, CancellationToken ct = default)
    {
        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(role => role.RoleId == roleId, ct);

        if (role is null)
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        var rolePermissions = await _dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .ToListAsync(ct);

        _dbContext.RolePermissions.RemoveRange(rolePermissions);
        _dbContext.Roles.Remove(role);

    }
}
