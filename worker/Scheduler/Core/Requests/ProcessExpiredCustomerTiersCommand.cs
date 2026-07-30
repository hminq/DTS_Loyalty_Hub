using Scheduler.Core.Abstractions;
using MediatR;

namespace Scheduler.Core.Requests;

public sealed record ProcessExpiredCustomerTierBatchCommand(
    DateTime ProcessedAt,
    int BatchSize)
    : IRequest<ProcessExpiredCustomerTierBatchResult>, ITransactionalRequest;

public sealed record ProcessExpiredCustomerTierBatchResult(
    int SelectedCount,
    int ProcessedCount);
