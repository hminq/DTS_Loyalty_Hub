namespace Api.Dtos.Responses.Auth;

public sealed class LoginResponseDto
{
    public string AccessToken { get; set; } = null!;

    public string TokenType { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public AdminLoginResponseDto Admin { get; set; } = null!;

    public IReadOnlyCollection<string> Permissions { get; set; } = [];
}

public sealed class AdminLoginResponseDto
{
    public Guid UserId { get; set; }

    public Guid AdminId { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = null!;
}
