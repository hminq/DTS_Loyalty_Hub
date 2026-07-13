using Core.Abstractions;

namespace Infrastructure.Auth;

public sealed class PasswordVerifier : IPasswordVerifier
{
    public bool Verify(string passwordHash, string password)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
