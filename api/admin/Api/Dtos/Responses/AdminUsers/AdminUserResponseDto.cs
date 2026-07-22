namespace Api.Dtos.Responses.AdminUsers;

public sealed class AdminUserListItemResponseDto
{
    public Guid AdminId { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

public sealed class AdminUserRoleResponseDto
{
    public Guid RoleId { get; set; }

    public string Name { get; set; } = null!;

    public IReadOnlyCollection<AdminUserPermissionResponseDto> Permissions { get; set; } = [];
}

public sealed class AdminUserPermissionResponseDto
{
    public Guid PermissionId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string GroupCode { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string ActionCode { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public int GroupSortOrder { get; set; }

    public int ActionSortOrder { get; set; }
}

public sealed class AdminUserResponseDto
{
    public Guid AdminId { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public AdminUserRoleResponseDto? Role { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
