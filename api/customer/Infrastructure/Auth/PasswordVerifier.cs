using System.Security.Cryptography;
using System.Text;
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

        if (IsBcryptHash(passwordHash))
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        var legacyHash = Convert.ToHexString(bytes);

        return string.Equals(passwordHash, legacyHash, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBcryptHash(string passwordHash)
    {
        return passwordHash.StartsWith("$2a$", StringComparison.Ordinal)
            || passwordHash.StartsWith("$2b$", StringComparison.Ordinal)
            || passwordHash.StartsWith("$2y$", StringComparison.Ordinal);
    }
<<<<<<< HEAD

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
=======
>>>>>>> origin/feature/admin-auth
}
