using Scheduler.Core.Abstractions;
using Scheduler.Core.Entities;
using MediatR;

namespace Scheduler.Core.Requests;

public sealed record StageVoucherPoolImportBatchCommand(
    Guid JobId,
    int StartProcessedCount,
    IReadOnlyCollection<VoucherPoolImportRawRow> Rows,
    DateTime ProcessedAt) : IRequest<StageVoucherPoolImportBatchResult>, ITransactionalRequest;

public sealed record StageVoucherPoolImportBatchResult(
    int StagedCount,
    int ProcessedCount);
