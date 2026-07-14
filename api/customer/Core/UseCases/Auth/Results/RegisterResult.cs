namespace Core.UseCases.Auth.Results;

public sealed record RegisterResult(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt,
    CustomerRegisterResult Customer);

public sealed record CustomerRegisterResult(
    Guid UserId,
    Guid CustomerId,
    string Username,
    string Email,
    string FullName);
