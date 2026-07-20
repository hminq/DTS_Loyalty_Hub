using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IAdminSessionRepository
{
    Task<AdminLoginSession?> CreateSessionIfNoneActiveAsync(
        Guid adminId,
        Guid userId,
        DateTime expiresAt,
        CancellationToken ct = default);

    Task<bool> IsSessionActiveAsync(
        Guid adminSessionId,
        Guid accessTokenJti,
        Guid adminId,
        Guid userId,
        CancellationToken ct = default);

    Task RevokeSessionAsync(
        Guid adminId,
        Guid adminSessionId,
        Guid accessTokenJti,
        CancellationToken ct = default);

    Task RevokeActiveSessionsAsync(Guid adminId, CancellationToken ct = default);
}
