using Core.Abstractions;
using Core.UseCases.AuditLogs.Results;
using Core.UseCases.Common;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public AuditLogRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AuditLogResult>> GetPagedAsync(
        int page,
        int pageSize,
        DateTime? fromDate,
        DateTime? toDate,
        string? entityType,
        string? action,
        CancellationToken ct = default)
    {
        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(auditLog => auditLog.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(auditLog => auditLog.CreatedAt <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var normalizedEntityType = entityType.Trim();
            query = query.Where(auditLog => auditLog.EntityType == normalizedEntityType);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            var normalizedAction = action.Trim().ToUpperInvariant();
            query = query.Where(auditLog => auditLog.Action == normalizedAction);
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(auditLog => auditLog.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(auditLog => new AuditLogResult(
                auditLog.AuditLogId,
                auditLog.ActorUserId,
                auditLog.Action,
                auditLog.EntityType,
                auditLog.EntityId,
                auditLog.OldValue,
                auditLog.NewValue,
                auditLog.Metadata,
                auditLog.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<AuditLogResult>(items, page, pageSize, totalItems);
    }
}
