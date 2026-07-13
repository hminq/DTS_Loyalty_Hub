using Core.Entities.Constants;
using Core.Abstractions;
using Core.UseCases.Auth.Models;
using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

public sealed class UserRepository : IUserRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public UserRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminLoginUser?> GetByUsernameAsync(
        string username,
        CancellationToken ct = default)
    {
        var admin = await _dbContext.Admins
            .AsNoTracking()
            .Where(admin => admin.User.Username == username && admin.User.UserType == UserTypes.Admin)
            .Select(admin => new
            {
                admin.AdminId,
                admin.User.UserId,
                admin.User.Username,
                admin.User.FullName,
                admin.User.PasswordHash,
                admin.User.Status,
                admin.Role.RoleId,
                RoleName = admin.Role.Name,
                PermissionCodes = admin.Role.RolePermissions
                    .Select(rolePermission => rolePermission.Permission.Code)
                    .OrderBy(code => code)
                    .ToList()
            })
            .SingleOrDefaultAsync(ct);

        if (admin is null)
        {
            return null;
        }

        return new AdminLoginUser(
            admin.UserId,
            admin.AdminId,
            admin.Username,
            admin.FullName ?? string.Empty,
            admin.PasswordHash,
            admin.Status,
            admin.RoleId,
            admin.RoleName,
            admin.PermissionCodes);
    }
}
