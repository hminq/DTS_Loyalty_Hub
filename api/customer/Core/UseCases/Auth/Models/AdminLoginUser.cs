namespace Core.UseCases.Auth.Models;

public sealed record CustomerLoginUser(
    Guid UserId,
    Guid CustomerId,
    string Username,
    string FullName,
    string PasswordHash,
    string Status,
    Guid RoleId,
    string RoleName,
    IReadOnlyCollection<string> PermissionCodes);
