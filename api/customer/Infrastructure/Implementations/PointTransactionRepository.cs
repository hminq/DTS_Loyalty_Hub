using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.Customers.Queries.GetPointTransactions;
using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

public sealed class PointTransactionRepository : IPointTransactionRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public PointTransactionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PointTransactionResult>> GetPagedByCustomerIdAsync(
        Guid customerId, int pageIndex, int pageSize, CancellationToken ct)
    {
        var query = _dbContext.PointTransactions
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PointTransactionResult(
                x.PointTransactionId,
                x.TransactionType,
                x.Amount,
                x.BalanceBefore,
                x.BalanceAfter,
                x.CreatedAt,
                x.SourceEventId,
                x.CampaignSessionId
            ))
            .ToListAsync(ct);

        return new PagedResult<PointTransactionResult>(items, totalCount);
    }
}
