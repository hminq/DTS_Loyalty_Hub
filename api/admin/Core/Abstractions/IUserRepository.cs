using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IUserRepository
{
    Task<AdminLoginUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
