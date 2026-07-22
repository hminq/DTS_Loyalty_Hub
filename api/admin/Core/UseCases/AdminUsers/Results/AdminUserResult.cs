namespace Core.UseCases.AdminUsers.Results;

public sealed record AdminUserResult(
    Guid AdminId,
    Guid UserId,
    string Username,
    string Email,
    string FullName,
    string? PhoneNumber,
    Guid RoleId,
    string RoleName,
    string Status,
    DateTime CreatedAt,
    AdminUserRoleResult? Role = null);

public sealed record AdminUserRoleResult(
    Guid RoleId,
    string Name,
    IReadOnlyCollection<AdminUserPermissionResult> Permissions);

public sealed record AdminUserPermissionResult(
    Guid PermissionId,
    string Code,
    string Name,
    string GroupCode,
    string GroupName,
    string ActionCode,
    string ActionName,
    int GroupSortOrder,
    int ActionSortOrder);
