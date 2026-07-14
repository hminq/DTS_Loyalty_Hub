using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.Roles.Results;
using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;
using DomainRole = Core.Entities.Role;
using PersistenceRole = Infrastructure.Models.Role;
using PersistenceRolePermission = Infrastructure.Models.RolePermission;

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

    public Task<bool> HasAssignedAdminsAsync(Guid roleId, CancellationToken ct = default)
    {
        return _dbContext.Admins
            .AsNoTracking()
            .AnyAsync(admin => admin.RoleId == roleId, ct);
    }

    public async Task<IReadOnlySet<Guid>> GetExistingPermissionIdsAsync(
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken ct = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .Where(permission => permissionIds.Contains(permission.PermissionId))
            .Select(permission => permission.PermissionId)
            .ToHashSetAsync(ct);
    }

    public async Task<DomainRole> CreateAsync(DomainRole role, CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

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

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return role;
    }

    public async Task<DomainRole> UpdateAsync(DomainRole role, CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var persistedRole = await _dbContext.Roles
            .FirstOrDefaultAsync(persistedRole => persistedRole.RoleId == role.RoleId, ct);

        if (persistedRole is null)
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                "Role does not exist.",
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

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return role;
    }

    public async Task DeleteAsync(Guid roleId, CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(role => role.RoleId == roleId, ct);

        if (role is null)
        {
            throw new DomainException(
                "ROLE_NOT_FOUND",
                "Role does not exist.",
                DomainErrorType.NotFound);
        }

        var rolePermissions = await _dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .ToListAsync(ct);

        _dbContext.RolePermissions.RemoveRange(rolePermissions);
        _dbContext.Roles.Remove(role);

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
