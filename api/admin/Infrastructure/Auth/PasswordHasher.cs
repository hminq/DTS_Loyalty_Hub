using Core.Abstractions;

namespace Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
