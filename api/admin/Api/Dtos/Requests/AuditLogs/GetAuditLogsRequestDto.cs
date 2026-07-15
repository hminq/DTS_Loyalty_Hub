namespace Api.Dtos.Requests.AuditLogs;

public sealed class GetAuditLogsRequestDto
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? EntityType { get; set; }

    public string? Action { get; set; }
}