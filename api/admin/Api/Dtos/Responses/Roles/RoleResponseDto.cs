namespace Api.Dtos.Responses.Roles;

public sealed class RoleResponseDto
{
    public Guid RoleId { get; set; }

    public string Name { get; set; } = null!;

    public IReadOnlyCollection<Guid> PermissionIds { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}
