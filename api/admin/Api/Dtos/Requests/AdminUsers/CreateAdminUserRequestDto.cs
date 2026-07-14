namespace Api.Dtos.Requests.AdminUsers;

public sealed class CreateAdminUserRequestDto
{
    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public Guid RoleId { get; set; }
}
