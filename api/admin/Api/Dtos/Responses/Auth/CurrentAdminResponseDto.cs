namespace Api.Dtos.Responses.Auth;

public sealed class CurrentAdminResponseDto
{
    public Guid UserId { get; set; }

    public Guid AdminId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public IReadOnlyCollection<string> Permissions { get; set; } = [];
}
