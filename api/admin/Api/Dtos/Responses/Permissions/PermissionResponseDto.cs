namespace Api.Dtos.Responses.Permissions;

public sealed class PermissionResponseDto
{
    public Guid PermissionId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string ActionCode { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public int ActionSortOrder { get; set; }
}
