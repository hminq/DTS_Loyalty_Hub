namespace Core.UseCases.AuditLogs.Results;

public sealed record AuditLogFilterOptionsResult(
    IReadOnlyCollection<string> EntityTypes,
    IReadOnlyCollection<string> Actions);
