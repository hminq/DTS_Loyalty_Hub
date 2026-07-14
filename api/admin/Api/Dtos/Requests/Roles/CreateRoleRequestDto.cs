namespace Api.Dtos.Requests.Roles;

public sealed class CreateRoleRequestDto
{
    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<Guid> PermissionIds { get; set; } = [];
}
