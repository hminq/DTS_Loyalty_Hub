namespace Core.Abstractions;

public interface IPasswordVerifier
{
    bool Verify(string passwordHash, string password);
<<<<<<< HEAD

    string Hash(string password);
=======
>>>>>>> origin/feature/admin-auth
}
