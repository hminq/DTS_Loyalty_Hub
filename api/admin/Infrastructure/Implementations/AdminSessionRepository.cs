using Core.Abstractions;
using Core.UseCases.Auth.Models;
using Infrastructure.Models;
using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

public sealed class AdminSessionRepository : IAdminSessionRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public AdminSessionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminLoginSession> ReplaceActiveSessionAsync(
        Guid adminId,
        Guid userId,
        DateTime expiresAt,
        CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var now = DateTime.UtcNow;
        var activeSessions = await _dbContext.AdminSessions
            .Where(session =>
                session.AdminId == adminId &&
                session.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var activeSession in activeSessions)
        {
            activeSession.RevokedAt = now;
        }

        if (activeSessions.Count > 0)
        {
            await _dbContext.SaveChangesAsync(ct);
        }

        var adminSession = new AdminSession
        {
            AdminSessionId = Guid.NewGuid(),
            AdminId = adminId,
            UserId = userId,
            AccessTokenJti = Guid.NewGuid(),
            ExpiresAt = expiresAt,
            CreatedAt = now
        };

        _dbContext.AdminSessions.Add(adminSession);

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new AdminLoginSession(
            adminSession.AdminSessionId,
            adminSession.AccessTokenJti,
            adminSession.ExpiresAt);
    }

    public Task<bool> IsSessionActiveAsync(
        Guid adminSessionId,
        Guid accessTokenJti,
        Guid adminId,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return _dbContext.AdminSessions
            .AsNoTracking()
            .AnyAsync(session =>
                session.AdminSessionId == adminSessionId &&
                session.AccessTokenJti == accessTokenJti &&
                session.AdminId == adminId &&
                session.RevokedAt == null &&
                session.ExpiresAt > now,
                ct);
    }
}
