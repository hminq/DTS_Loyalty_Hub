using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IUserRepository
{
    Task<CustomerLoginUser?> GetByUsernameAsync(string username, CancellationToken ct);

    Task<CustomerLoginUser?> GetByIdAsync(Guid userId, CancellationToken ct);

    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);

    Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct);

    CreatedCustomerUser Add(Guid userId, Guid customerId, NewCustomerUser newUser);
}
