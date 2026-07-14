using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IAdminSessionRepository
{
    Task<AdminLoginSession> ReplaceActiveSessionAsync(
        Guid adminId,
        Guid userId,
        DateTime expiresAt,
        CancellationToken ct = default);

    Task<bool> IsSessionActiveAsync(
        Guid adminSessionId,
        Guid accessTokenJti,
        Guid adminId,
        CancellationToken ct = default);
}
