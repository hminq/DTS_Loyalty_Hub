using Core.Abstractions;
using Core.UseCases.Auth.Models;
using Persistence.Models;
using Persistence.Models.Context;
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

        return new AdminLoginSession(
            adminSession.AdminSessionId,
            adminSession.AccessTokenJti,
            adminSession.ExpiresAt);
    }

    public async Task RevokeActiveSessionsAsync(Guid adminId, CancellationToken ct = default)
    {
        var sessions = await _dbContext.AdminSessions
            .Where(session => session.AdminId == adminId && session.RevokedAt == null)
            .ToListAsync(ct);
        var now = DateTime.UtcNow;

        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }
    }

    public Task<bool> IsSessionActiveAsync(
        Guid adminSessionId,
        Guid accessTokenJti,
        Guid adminId,
        Guid userId,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return _dbContext.AdminSessions
            .AsNoTracking()
            .AnyAsync(session =>
                session.AdminSessionId == adminSessionId &&
                session.AccessTokenJti == accessTokenJti &&
                session.AdminId == adminId &&
                session.UserId == userId &&
                session.RevokedAt == null &&
                session.ExpiresAt > now,
                ct);
    }
}
