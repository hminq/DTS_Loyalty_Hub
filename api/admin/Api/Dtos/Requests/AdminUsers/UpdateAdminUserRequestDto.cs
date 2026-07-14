namespace Api.Dtos.Requests.AdminUsers;

public sealed class UpdateAdminUserRequestDto
{
    public string? Email { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public Guid RoleId { get; set; }
}
