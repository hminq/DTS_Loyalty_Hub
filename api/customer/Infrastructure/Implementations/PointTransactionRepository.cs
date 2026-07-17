using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.Customers.Queries.GetPointTransactions;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class PointTransactionRepository : IPointTransactionRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public PointTransactionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PointTransactionResult>> GetPagedByCustomerIdAsync(
        Guid customerId, 
        int pageIndex, 
        int pageSize, 
        string? transactionType,
        DateTime? fromDate,
        DateTime? toDate,
        decimal? minAmount,
        decimal? maxAmount,
        CancellationToken ct)
    {
        var query = _dbContext.PointTransactions
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId);

        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            var typeStr = transactionType.Trim().ToUpper();
            query = query.Where(x => x.TransactionType.ToUpper() == typeStr);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= toDate.Value);
        }

        if (minAmount.HasValue)
        {
            query = query.Where(x => x.Amount >= minAmount.Value);
        }

        if (maxAmount.HasValue)
        {
            query = query.Where(x => x.Amount <= maxAmount.Value);
        }

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

        return new PagedResult<PointTransactionResult>(items, totalCount, pageIndex, pageSize);
    }
}
