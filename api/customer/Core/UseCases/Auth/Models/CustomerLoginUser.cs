namespace Core.UseCases.Auth.Models;

public sealed record CustomerLoginUser(
    Guid UserId,
    Guid CustomerId,
    string Username,
    string FullName,
    string PasswordHash,
    string Status)
{
    public CustomerTokenUser ToTokenUser()
    {
        return new CustomerTokenUser(UserId, CustomerId, Username);
    }
}

public sealed record CustomerTokenUser(
    Guid UserId,
    Guid CustomerId,
    string Username);
