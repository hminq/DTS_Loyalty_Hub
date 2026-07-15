namespace Core.UseCases.AuditLogs;

public sealed record AuditLogEntry(
    Guid? ActorUserId,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? OldValue,
    string? NewValue,
    string? Metadata);