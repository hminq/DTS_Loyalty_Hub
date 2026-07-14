namespace Core.UseCases.Auth.Results;

public sealed record RegisterResult(
    Guid UserId,
    Guid CustomerId,
    string Username,
    string Email,
    string FullName);
