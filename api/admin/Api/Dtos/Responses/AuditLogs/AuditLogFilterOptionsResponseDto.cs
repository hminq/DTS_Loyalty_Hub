namespace Api.Dtos.Responses.AuditLogs;

public sealed record AuditLogFilterOptionsResponseDto(
    IReadOnlyCollection<string> EntityTypes,
    IReadOnlyCollection<string> Actions);
