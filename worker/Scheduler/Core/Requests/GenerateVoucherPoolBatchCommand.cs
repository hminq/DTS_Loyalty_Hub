using Scheduler.Core.Abstractions;
using MediatR;

namespace Scheduler.Core.Requests;

public sealed record GenerateVoucherPoolBatchCommand(
    Guid JobId,
    int BatchSize,
    DateTime ProcessedAt) : IRequest<GenerateVoucherPoolBatchResult>, ITransactionalRequest;

public sealed record GenerateVoucherPoolBatchResult(
    int GeneratedCount,
    int ProcessedCount,
    bool IsCompleted);
