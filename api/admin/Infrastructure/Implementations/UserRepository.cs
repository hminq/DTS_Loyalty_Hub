using Core.Entities.Constants;
using Core.Entities;
using Core.Abstractions;
using Core.UseCases.Auth.Models;
using Persistence.Models.Context;
using Microsoft.EntityFrameworkCore;
using Core.Exceptions;
using Persistence.Models;

namespace Infrastructure.Implementations;

public sealed class UserRepository : IUserRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public UserRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminLoginUser?> GetByUsernameAsync(
        string username,
        CancellationToken ct = default)
    {
        var admin = await _dbContext.Admins
            .AsNoTracking()
            .Where(admin => admin.User.Username == username && admin.User.UserType == UserTypes.Admin)
            .Select(admin => new
            {
                admin.AdminId,
                admin.User.UserId,
                admin.User.Username,
                admin.User.FullName,
                admin.User.PasswordHash,
                admin.User.Status,
                admin.Role.RoleId,
                RoleName = admin.Role.Name,
                PermissionCodes = admin.Role.RolePermissions
                    .Select(rolePermission => rolePermission.Permission.Code)
                    .OrderBy(code => code)
                    .ToList()
            })
            .SingleOrDefaultAsync(ct);

        if (admin is null)
        {
            return null;
        }

        return new AdminLoginUser(
            admin.UserId,
            admin.AdminId,
            admin.Username,
            admin.FullName ?? string.Empty,
            admin.PasswordHash,
            admin.Status,
            admin.RoleId,
            admin.RoleName,
            admin.PermissionCodes);
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default) =>
        _dbContext.Users.AsNoTracking().AnyAsync(user => user.Username == username.Trim(), ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = UserProfileRules.NormalizeEmail(email);
        return _dbContext.Users.AsNoTracking()
            .AnyAsync(user => user.Email.ToLower() == normalizedEmail, ct);
    }

    public Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken ct = default)
    {
        var normalizedPhoneNumber = UserProfileRules.NormalizeOptionalPhoneNumber(phoneNumber)!;
        return _dbContext.Users.AsNoTracking()
            .AnyAsync(user => user.PhoneNumber == normalizedPhoneNumber, ct);
    }

    public Task<bool> EmailExistsExceptAdminAsync(
        string email,
        Guid adminId,
        CancellationToken ct = default)
    {
        var normalizedEmail = UserProfileRules.NormalizeEmail(email);
        return _dbContext.Users.AsNoTracking().AnyAsync(
            user => user.Email.ToLower() == normalizedEmail &&
                (user.Admin == null || user.Admin.AdminId != adminId),
            ct);
    }

    public Task<bool> PhoneNumberExistsExceptAdminAsync(
        string phoneNumber,
        Guid adminId,
        CancellationToken ct = default)
    {
        var normalizedPhoneNumber = UserProfileRules.NormalizeOptionalPhoneNumber(phoneNumber)!;
        return _dbContext.Users.AsNoTracking().AnyAsync(
            user => user.PhoneNumber == normalizedPhoneNumber &&
                (user.Admin == null || user.Admin.AdminId != adminId),
            ct);
    }

    public Task<bool> EmailExistsExceptCustomerAsync(
        string email,
        Guid customerId,
        CancellationToken ct = default)
    {
        var normalizedEmail = UserProfileRules.NormalizeEmail(email);
        return _dbContext.Users.AsNoTracking().AnyAsync(
            user => user.Email.ToLower() == normalizedEmail &&
                (user.Customer == null || user.Customer.CustomerId != customerId),
            ct);
    }

    public Task<bool> PhoneNumberExistsExceptCustomerAsync(
        string phoneNumber,
        Guid customerId,
        CancellationToken ct = default)
    {
        var normalizedPhoneNumber = UserProfileRules.NormalizeOptionalPhoneNumber(phoneNumber)!;
        return _dbContext.Users.AsNoTracking().AnyAsync(
            user => user.PhoneNumber == normalizedPhoneNumber &&
                (user.Customer == null || user.Customer.CustomerId != customerId),
            ct);
    }

    public void AddAdminUser(
        Guid userId,
        string username,
        string email,
        string passwordHash,
        string? fullName,
        string? phoneNumber,
        DateTime createdAt)
    {
        _dbContext.Users.Add(new User
        {
            UserId = userId,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            UserType = UserTypes.Admin,
            Status = UserStatus.Enable,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });
    }

    public async Task UpdateAdminProfileAsync(
        Guid adminId,
        string email,
        string? fullName,
        string? phoneNumber,
        CancellationToken ct = default)
    {
        var user = await GetAdminUserForUpdateAsync(adminId, ct);
        user.Email = email;
        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
    }

    public async Task UpdateAdminStatusAsync(Guid adminId, string status, CancellationToken ct = default)
    {
        var user = await GetAdminUserForUpdateAsync(adminId, ct);
        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;
    }

    public async Task UpdateCustomerProfileAsync(
        Guid customerId,
        string email,
        string? fullName,
        string? phoneNumber,
        CancellationToken ct = default)
    {
        var user = await GetCustomerUserForUpdateAsync(customerId, ct);
        user.Email = email;
        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
    }

    public async Task UpdateCustomerStatusAsync(
        Guid customerId,
        string status,
        CancellationToken ct = default)
    {
        var user = await GetCustomerUserForUpdateAsync(customerId, ct);
        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<User> GetAdminUserForUpdateAsync(Guid adminId, CancellationToken ct)
    {
        return await _dbContext.Admins
            .Where(admin => admin.AdminId == adminId)
            .Select(admin => admin.User)
            .SingleOrDefaultAsync(ct)
            ?? throw new DomainException("ADMIN_USER_NOT_FOUND", DomainErrorType.NotFound);
    }

    private async Task<User> GetCustomerUserForUpdateAsync(Guid customerId, CancellationToken ct)
    {
        return await _dbContext.Customers
            .Where(customer =>
                customer.CustomerId == customerId &&
                customer.User.UserType == UserTypes.Customer)
            .Select(customer => customer.User)
            .SingleOrDefaultAsync(ct)
            ?? throw new DomainException("CUSTOMER_USER_NOT_FOUND", DomainErrorType.NotFound);
    }
}
