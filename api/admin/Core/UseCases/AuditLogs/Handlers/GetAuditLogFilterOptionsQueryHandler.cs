using Core.Entities.Constants;
using Core.UseCases.AuditLogs.Queries;
using Core.UseCases.AuditLogs.Results;
using MediatR;

namespace Core.UseCases.AuditLogs.Handlers;

public sealed class GetAuditLogFilterOptionsQueryHandler
    : IRequestHandler<GetAuditLogFilterOptionsQuery, AuditLogFilterOptionsResult>
{
    public Task<AuditLogFilterOptionsResult> Handle(
        GetAuditLogFilterOptionsQuery request,
        CancellationToken ct)
    {
        var result = new AuditLogFilterOptionsResult(
            AuditEntityTypes.All.Order(StringComparer.Ordinal).ToArray(),
            AuditActions.All.Order(StringComparer.Ordinal).ToArray());

        return Task.FromResult(result);
    }
}
