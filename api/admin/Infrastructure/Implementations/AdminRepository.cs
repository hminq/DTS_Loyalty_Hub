using Core.Abstractions;
using Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class AdminRepository : IAdminRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public AdminRepository(LoyaltyHubDbContext dbContext) => _dbContext = dbContext;

    public void Add(
        Guid adminId,
        Guid userId,
        Guid roleId,
        DateTime createdAt)
    {
        _dbContext.Admins.Add(new Admin
        {
            AdminId = adminId,
            UserId = userId,
            RoleId = roleId,
            CreatedAt = createdAt
        });
    }

    public async Task UpdateRoleAsync(Guid adminId, Guid roleId, CancellationToken ct = default)
    {
        var admin = await _dbContext.Admins.SingleOrDefaultAsync(x => x.AdminId == adminId, ct)
            ?? throw new DomainException("ADMIN_USER_NOT_FOUND", "Admin user does not exist.", DomainErrorType.NotFound);

        admin.RoleId = roleId;
    }
}
