namespace Api.Dtos.Responses.Permissions;

public sealed class PermissionGroupResponseDto
{
    public string GroupCode { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public int GroupSortOrder { get; set; }

    public IReadOnlyCollection<PermissionResponseDto> Permissions { get; set; } = [];
}
