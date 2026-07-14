namespace Api.Dtos.Responses.Auth;

public sealed class RegisterResponseDto
{
    public Guid UserId { get; set; }

    public Guid CustomerId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;
}
