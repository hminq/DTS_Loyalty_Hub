namespace Api.Authentication;

public sealed record AuthenticatedAdminSession(
    Guid UserId,
    Guid AdminId,
    Guid AdminSessionId,
    Guid AccessTokenJti);
