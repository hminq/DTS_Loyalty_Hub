using Core.Abstractions;
using Core.UseCases.AuditLogs;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Auditing;

public sealed class AuditLogWriter : IAuditLogWriter
{
    private readonly LoyaltyHubDbContext _dbContext;

    public AuditLogWriter(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(AuditLogEntry entry)
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
            Metadata = entry.Metadata ?? "{}",
            CreatedAt = DateTime.UtcNow
        });
    }
}
