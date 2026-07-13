namespace Core.Abstractions;

public interface IPasswordVerifier
{
    bool Verify(string passwordHash, string password);
}
