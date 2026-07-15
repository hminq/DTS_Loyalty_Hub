using Core.UseCases.AuditLogs.Results;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.AuditLogs.Queries;

public sealed record GetAuditLogsQuery(
    int Page,
    int PageSize,
    DateTime? FromDate,
    DateTime? ToDate,
    string? EntityType,
    string? Action) : IRequest<PagedResult<AuditLogResult>>;