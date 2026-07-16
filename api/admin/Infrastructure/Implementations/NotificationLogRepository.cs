using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.Notifications.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class NotificationLogRepository : INotificationLogRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public NotificationLogRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<NotificationLogResult>> GetPagedAsync(
        int page, 
        int pageSize, 
        Guid? customerId = null, 
        string? eventTypeCode = null, 
        CancellationToken ct = default)
    {
        var query = _dbContext.NotificationLogs
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(l => l.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(eventTypeCode))
        {
            query = query.Where(l => l.EventTypeCode == eventTypeCode);
        }

        var totalItems = await query.CountAsync(ct);

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new NotificationLogResult(
                x.LogId,
                x.TemplateId,
                x.EventTypeCode,
                x.Channel,
                x.CustomerId,
                x.RenderedTitle,
                x.RenderedBody,
                x.VariablesSnapshot,
                x.Status,
                x.CreatedAt,
                x.SentAt))
            .ToListAsync(ct);

        return new PagedResult<NotificationLogResult>(logs, page, pageSize, totalItems);
    }
}
