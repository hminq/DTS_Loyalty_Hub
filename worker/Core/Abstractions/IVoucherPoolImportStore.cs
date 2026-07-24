using Core.Entities;

namespace Core.Abstractions;

public interface IVoucherPoolImportStore
{
    Task<int> CountStagedRowsAsync(Guid jobId, CancellationToken cancellationToken);

    Task<IReadOnlySet<string>> FindStagedCodesAsync(
        Guid jobId,
        IReadOnlyCollection<string> voucherCodes,
        CancellationToken cancellationToken);

    Task BulkInsertStagingAsync(
        IReadOnlyCollection<VoucherPoolImportMutation> rows,
        CancellationToken cancellationToken);

    Task<string?> FindGlobalConflictAsync(
        Guid jobId,
        CancellationToken cancellationToken);

    Task PromoteAsync(
        Guid jobId,
        DateTime completedAt,
        CancellationToken cancellationToken);
}
