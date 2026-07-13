using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IUserRepository
{
    Task<CustomerLoginUser?> GetByUsernameAsync(string username, CancellationToken ct);
}
