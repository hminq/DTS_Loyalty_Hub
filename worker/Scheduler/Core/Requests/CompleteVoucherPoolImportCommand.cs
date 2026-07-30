using Scheduler.Core.Abstractions;
using MediatR;

namespace Scheduler.Core.Requests;

public sealed record CompleteVoucherPoolImportCommand(
    Guid JobId,
    DateTime CompletedAt) : IRequest, ITransactionalRequest;
