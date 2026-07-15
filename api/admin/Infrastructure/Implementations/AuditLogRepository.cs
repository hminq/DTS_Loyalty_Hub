using Core.Abstractions;
using Core.UseCases.AuditLogs;
using Core.UseCases.AuditLogs.Results;
using Core.UseCases.Common;
using Infrastructure.Models;
using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementations;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public AuditLogRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            ActorUserId = entry.ActorUserId,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            OldValue = entry.OldValue,
            NewValue = entry.NewValue,
            Metadata = entry.Metadata ?? "{}", // cột metadata là jsonb NOT NULL default '{}'
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(ct);
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