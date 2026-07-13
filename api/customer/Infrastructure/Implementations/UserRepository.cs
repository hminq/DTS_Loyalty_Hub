using Core.Entities.Constants;
using Core.Abstractions;
using Core.UseCases.Auth.Models;
using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;

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
