using Core.UseCases.AuditLogs.Results;
using MediatR;

namespace Core.UseCases.AuditLogs.Queries;

public sealed record GetAuditLogFilterOptionsQuery : IRequest<AuditLogFilterOptionsResult>;
