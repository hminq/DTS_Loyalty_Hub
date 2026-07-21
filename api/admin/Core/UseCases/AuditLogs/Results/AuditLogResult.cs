namespace Core.UseCases.AuditLogs.Results;

public sealed record AuditLogResult(
    Guid AuditLogId,
    Guid? ActorUserId,
    string? ActorUsername,
    string? ActorFullName,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? OldValue,
    string? NewValue,
    string? Metadata,
    DateTime CreatedAt);
