using Core.Entities.Constants;
using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Auth.Models;
using Infrastructure.Models;
using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Implementations;

public sealed class UserRepository : IUserRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public UserRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerLoginUser?> GetByUsernameAsync(
        string username,
        CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Username == username && u.UserType == UserTypes.Customer)
            .SingleOrDefaultAsync(ct);

        return await ToCustomerLoginUserAsync(user, ct);
    }

    public async Task<CustomerLoginUser?> GetByIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId && u.UserType == UserTypes.Customer)
            .SingleOrDefaultAsync(ct);

        return await ToCustomerLoginUserAsync(user, ct);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Username == username, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email, ct);
    }

    public async Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct = default)
    {
        return await _dbContext.Users.AsNoTracking().AnyAsync(u => u.PhoneNumber == phone, ct);
    }

    public async Task<CreatedCustomerUser> CreateAsync(NewCustomerUser newUser, CancellationToken ct = default)
    {
        var user = new User
        {
            Username = newUser.Username,
            Email = newUser.Email,
            PasswordHash = newUser.PasswordHash,
            FullName = newUser.FullName,
            PhoneNumber = newUser.Phone,
            UserType = UserTypes.Customer,
            Status = UserStatus.Enable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var customer = new Customer
        {
            User = user,
            TierId = null,
            CurrentTierPoint = 0,
            NextTierPoint = 0,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.Customers.Add(customer);

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new DomainException(
                "DUPLICATE_USER_DATA",
                "Username, email or phone already exists.",
                DomainErrorType.Conflict);
        }

        return new CreatedCustomerUser(user.UserId, customer.CustomerId);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }

    private async Task<CustomerLoginUser?> ToCustomerLoginUserAsync(User? user, CancellationToken ct)
    {
        if (user is null)
        {
            return null;
        }

        var customerId = await _dbContext.Customers
            .AsNoTracking()
            .Where(c => c.UserId == user.UserId)
            .Select(c => c.CustomerId)
            .FirstOrDefaultAsync(ct);

        if (customerId == Guid.Empty)
        {
            return null;
        }

        return new CustomerLoginUser(
            user.UserId,
            customerId,
            user.Username,
            user.FullName ?? string.Empty,
            user.PasswordHash,
            user.Status,
            Guid.Empty,
            string.Empty,
            []);
    }
}
