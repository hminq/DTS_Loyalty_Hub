namespace Core.UseCases.Auth.Models;

public sealed record AdminLoginSession(
    Guid AdminSessionId,
    Guid AccessTokenJti,
    DateTime ExpiresAt);
