using Core.UseCases.AuditLogs;
using Core.UseCases.AuditLogs.Results;
using Core.UseCases.Common;

namespace Core.Abstractions;

public interface IAuditLogRepository
{
    Task<PagedResult<AuditLogResult>> GetPagedAsync(
        int page,
        int pageSize,
        DateTime? fromDate,
        DateTime? toDate,
        string? entityType,
        string? action,
        CancellationToken ct = default);
}
