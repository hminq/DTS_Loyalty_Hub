namespace Core.UseCases.Auth.Results;

public sealed record CurrentAdminResult(
    Guid UserId,
    Guid AdminId,
    string Username,
    string Email,
    string FullName,
    string? PhoneNumber,
    Guid RoleId,
    string RoleName,
    string Status,
    IReadOnlyCollection<string> Permissions);
