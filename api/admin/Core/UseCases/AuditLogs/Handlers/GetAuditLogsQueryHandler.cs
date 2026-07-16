using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.AuditLogs.Queries;
using Core.UseCases.AuditLogs.Results;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.AuditLogs.Handlers;

public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogResult>>
{
    private const int MaxPageSize = 100;
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public Task<PagedResult<AuditLogResult>> Handle(GetAuditLogsQuery request, CancellationToken ct)
    {
        if (request.Page < 1)
        {
            throw new DomainException(
                "PAGE_INVALID",
                "Page must be greater than or equal to 1.",
                DomainErrorType.Validation);
        }

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
        {
            throw new DomainException(
                "PAGE_SIZE_INVALID",
                $"Page size must be between 1 and {MaxPageSize}.",
                DomainErrorType.Validation);
        }

        if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
        {
            throw new DomainException(
                "AUDIT_LOG_DATE_RANGE_INVALID",
                "From date must be earlier than or equal to to date.",
                DomainErrorType.Validation);
        }

        return _auditLogRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.FromDate,
            request.ToDate,
            request.EntityType,
            request.Action,
            ct);
    }
}
