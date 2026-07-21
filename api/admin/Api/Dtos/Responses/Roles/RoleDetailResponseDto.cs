namespace Api.Dtos.Responses.Roles;

/// <summary>Detailed role data including assigned permission metadata.</summary>
public sealed record RoleDetailResponseDto(
    Guid RoleId,
    string Name,
    IReadOnlyCollection<Guid> PermissionIds,
    IReadOnlyCollection<RolePermissionDetailResponseDto> Permissions,
    DateTime CreatedAt);

/// <summary>Permission metadata assigned to a role.</summary>
public sealed record RolePermissionDetailResponseDto(
    Guid PermissionId,
    string Code,
    string Name,
    string GroupCode,
    string GroupName,
    int GroupSortOrder,
    int ActionSortOrder);
