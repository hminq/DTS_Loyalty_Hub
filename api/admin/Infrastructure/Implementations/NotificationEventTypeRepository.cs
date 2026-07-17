using Core.Abstractions;
using Core.UseCases.Notifications.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class NotificationEventTypeRepository : INotificationEventTypeRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public NotificationEventTypeRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<NotificationEventTypeResult>> GetListAsync(string? keyword = null, CancellationToken ct = default)
    {
        var query = _dbContext.NotificationEventTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(x => x.DisplayName.ToLower().Contains(lowerKeyword) || x.EventTypeCode.ToLower().Contains(lowerKeyword));
        }

        return await query
            .Select(x => new NotificationEventTypeResult(
                x.NotificationEventTypeId,
                x.EventTypeCode,
                x.DisplayName,
                x.Description,
                x.AvailableVariables))
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.NotificationEventTypes.AnyAsync(x => x.NotificationEventTypeId == id, ct);
    }

    public async Task<NotificationEventTypeResult?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.NotificationEventTypes
            .AsNoTracking()
            .Where(x => x.NotificationEventTypeId == id)
            .Select(x => new NotificationEventTypeResult(
                x.NotificationEventTypeId,
                x.EventTypeCode,
                x.DisplayName,
                x.Description,
                x.AvailableVariables))
            .FirstOrDefaultAsync(ct);
    }
}
