namespace Core.UseCases.Roles.Results;

public sealed record RoleDetailResult(
    Guid RoleId,
    string Name,
    IReadOnlyCollection<RolePermissionDetailResult> Permissions,
    DateTime CreatedAt);

public sealed record RolePermissionDetailResult(
    Guid PermissionId,
    string Code,
    string Name,
    string GroupCode,
    string GroupName,
    int GroupSortOrder,
    int ActionSortOrder);
