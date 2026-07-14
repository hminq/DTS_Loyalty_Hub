namespace Api.Dtos.Responses.Auth;

public sealed class RegisterResponseDto
{
    public string AccessToken { get; set; } = null!;

    public string TokenType { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public CustomerRegisterResponseDto Customer { get; set; } = null!;
}

public sealed class CustomerRegisterResponseDto
{
    public Guid UserId { get; set; }

    public Guid CustomerId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;
}
