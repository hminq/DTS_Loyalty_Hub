using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AdminUsers.Results;
using Core.UseCases.Common;
using Persistence.Models;
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

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
    {
        var normalizedUsername = username.Trim();

        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Username == normalizedUsername, ct);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim();

        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email == normalizedEmail, ct);
    }

    public Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken ct = default)
    {
        var normalizedPhoneNumber = phoneNumber.Trim();

        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.PhoneNumber == normalizedPhoneNumber, ct);
    }

    public Task<bool> EmailExistsExceptAsync(
        string email,
        Guid adminId,
        CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim();

        return _dbContext.Admins
            .AsNoTracking()
            .AnyAsync(admin =>
                admin.AdminId != adminId &&
                admin.User.Email == normalizedEmail,
                ct);
    }

    public Task<bool> PhoneNumberExistsExceptAsync(
        string phoneNumber,
        Guid adminId,
        CancellationToken ct = default)
    {
        var normalizedPhoneNumber = phoneNumber.Trim();

        return _dbContext.Admins
            .AsNoTracking()
            .AnyAsync(admin =>
                admin.AdminId != adminId &&
                admin.User.PhoneNumber == normalizedPhoneNumber,
                ct);
    }

    public Task<bool> RoleExistsAsync(Guid roleId, CancellationToken ct = default)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .AnyAsync(role => role.RoleId == roleId, ct);
    }

    public async Task<AdminUserResult> CreateAsync(
        string username,
        string email,
        string passwordHash,
        string? fullName,
        string? phoneNumber,
        Guid roleId,
        CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var now = DateTime.UtcNow;
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            UserType = UserTypes.Admin,
            Status = UserStatus.Enable,
            CreatedAt = now,
            UpdatedAt = now
        };

        var admin = new Admin
        {
            AdminId = Guid.NewGuid(),
            UserId = user.UserId,
            RoleId = roleId,
            CreatedAt = now
        };

        _dbContext.Users.Add(user);
        _dbContext.Admins.Add(admin);

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var createdAdmin = await GetByIdAsync(admin.AdminId, ct);

        return createdAdmin ?? throw new DomainException(
            "ADMIN_USER_NOT_FOUND",
            "Admin user does not exist.",
            DomainErrorType.NotFound);
    }

    public async Task<AdminUserResult> UpdateAsync(
        Guid adminId,
        string email,
        string? fullName,
        string? phoneNumber,
        Guid roleId,
        CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var admin = await _dbContext.Admins
            .Include(admin => admin.User)
            .SingleOrDefaultAsync(admin => admin.AdminId == adminId, ct);

        if (admin is null)
        {
            throw new DomainException(
                "ADMIN_USER_NOT_FOUND",
                "Admin user does not exist.",
                DomainErrorType.NotFound);
        }

        admin.RoleId = roleId;
        admin.User.Email = email;
        admin.User.FullName = fullName;
        admin.User.PhoneNumber = phoneNumber;
        admin.User.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var updatedAdmin = await GetByIdAsync(adminId, ct);

        return updatedAdmin ?? throw new DomainException(
            "ADMIN_USER_NOT_FOUND",
            "Admin user does not exist.",
            DomainErrorType.NotFound);
    }

    public async Task UpdateStatusAsync(
        Guid adminId,
        string status,
        CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var admin = await _dbContext.Admins
            .Include(admin => admin.User)
            .SingleOrDefaultAsync(admin => admin.AdminId == adminId, ct);

        if (admin is null)
        {
            throw new DomainException(
                "ADMIN_USER_NOT_FOUND",
                "Admin user does not exist.",
                DomainErrorType.NotFound);
        }

        admin.User.Status = status;
        admin.User.UpdatedAt = DateTime.UtcNow;

        if (string.Equals(status, UserStatus.Disable, StringComparison.OrdinalIgnoreCase))
        {
            await RevokeActiveSessionsCoreAsync(adminId, ct);
        }

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task RevokeActiveSessionsAsync(Guid adminId, CancellationToken ct = default)
    {
        await RevokeActiveSessionsCoreAsync(adminId, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task RevokeActiveSessionsCoreAsync(Guid adminId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var sessions = await _dbContext.AdminSessions
            .Where(session =>
                session.AdminId == adminId &&
                session.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }
    }
}
