namespace Core.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
}
