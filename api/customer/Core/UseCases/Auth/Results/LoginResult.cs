namespace Core.UseCases.Auth.Results;

public sealed record LoginResult(
    string AccessToken,
    DateTime ExpiresAt,
    CustomerLoginResult Customer);

public sealed record CustomerLoginResult(
    Guid UserId,
    Guid CustomerId,
    string Username,
    string FullName);
