namespace Api.Dtos.Responses.AuditLogs;

public sealed class AuditLogResponseDto
{
    public Guid AuditLogId { get; set; }

    public Guid? ActorUserId { get; set; }

    public string Action { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public Guid? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }
}