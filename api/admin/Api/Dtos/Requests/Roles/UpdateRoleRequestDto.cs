namespace Api.Dtos.Requests.Roles;

public sealed class UpdateRoleRequestDto
{
    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<Guid> PermissionIds { get; set; } = [];
}
