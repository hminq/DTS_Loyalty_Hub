namespace Core.UseCases.Auth.Results;

public sealed record LoginResult(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt,
    AdminLoginResult Admin,
    IReadOnlyCollection<string> Permissions);

public sealed record AdminLoginResult(
    Guid UserId,
    Guid AdminId,
    string Username,
    string FullName,
    Guid RoleId,
    string RoleName);
