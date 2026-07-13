namespace Core.UseCases.Auth.Models;

public sealed record AdminLoginUser(
    Guid UserId,
    Guid AdminId,
    string Username,
    string FullName,
    string PasswordHash,
    string Status,
    Guid RoleId,
    string RoleName,
    IReadOnlyCollection<string> PermissionCodes);
