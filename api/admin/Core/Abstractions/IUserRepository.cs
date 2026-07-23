using Core.UseCases.Auth.Models;

namespace Core.Abstractions;

public interface IUserRepository
{
    Task<AdminLoginUser?> GetByUsernameAsync(string username, CancellationToken ct = default);

    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken ct = default);

    Task<bool> EmailExistsExceptAdminAsync(string email, Guid adminId, CancellationToken ct = default);

    Task<bool> PhoneNumberExistsExceptAdminAsync(string phoneNumber, Guid adminId, CancellationToken ct = default);

    Task<bool> EmailExistsExceptCustomerAsync(string email, Guid customerId, CancellationToken ct = default);

    Task<bool> PhoneNumberExistsExceptCustomerAsync(
        string phoneNumber,
        Guid customerId,
        CancellationToken ct = default);

    void AddAdminUser(
        Guid userId,
        string username,
        string email,
        string passwordHash,
        string? fullName,
        string? phoneNumber,
        DateTime createdAt);

    Task UpdateAdminProfileAsync(
        Guid adminId,
        string email,
        string? fullName,
        string? phoneNumber,
        CancellationToken ct = default);

    Task UpdateAdminStatusAsync(Guid adminId, string status, CancellationToken ct = default);

    Task UpdateCustomerProfileAsync(
        Guid customerId,
        string email,
        string? fullName,
        string? phoneNumber,
        CancellationToken ct = default);

    Task UpdateCustomerStatusAsync(Guid customerId, string status, CancellationToken ct = default);

}
