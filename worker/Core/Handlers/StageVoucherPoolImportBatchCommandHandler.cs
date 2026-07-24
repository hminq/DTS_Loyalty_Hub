using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class StageVoucherPoolImportBatchCommandHandler
    : IRequestHandler<StageVoucherPoolImportBatchCommand, StageVoucherPoolImportBatchResult>
{
    private const int MaximumCodeLength = 200;
    private readonly IVoucherPoolProvisioningRepository _repository;
    private readonly IVoucherPoolImportStore _importStore;

    public StageVoucherPoolImportBatchCommandHandler(
        IVoucherPoolProvisioningRepository repository,
        IVoucherPoolImportStore importStore)
    {
        _repository = repository;
        _importStore = importStore;
    }

    public async Task<StageVoucherPoolImportBatchResult> Handle(
        StageVoucherPoolImportBatchCommand request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetJobAsync(request.JobId, cancellationToken);
        if (job is null ||
            job.JobType != VoucherPoolProvisioningJobTypes.Imported ||
            job.Status != VoucherPoolProvisioningJobStatuses.Processing ||
            job.ProcessedCount != request.StartProcessedCount)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportStateInvalid);
        }

        var batchCodes = new HashSet<string>(StringComparer.Ordinal);
        var mutations = new List<VoucherPoolImportMutation>(request.Rows.Count);
        foreach (var row in request.Rows)
        {
            var code = row.RawCode?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new VoucherPoolImportException(
                    VoucherPoolGenerationErrorCodes.ImportCodeEmpty,
                    rowNumber: row.RowNumber);
            }

            if (code.Length > MaximumCodeLength)
            {
                throw new VoucherPoolImportException(
                    VoucherPoolGenerationErrorCodes.ImportCodeTooLong,
                    rowNumber: row.RowNumber);
            }

            if (!batchCodes.Add(code))
            {
                throw new VoucherPoolImportException(
                    VoucherPoolGenerationErrorCodes.ImportDuplicateInFile,
                    rowNumber: row.RowNumber);
            }

            mutations.Add(new VoucherPoolImportMutation(
                request.JobId,
                row.RowNumber,
                Guid.NewGuid(),
                code,
                request.ProcessedAt));
        }

        var stagedConflicts = await _importStore.FindStagedCodesAsync(
            request.JobId,
            batchCodes,
            cancellationToken);
        if (stagedConflicts.Count > 0)
        {
            var conflictRow = mutations.First(row =>
                stagedConflicts.Contains(row.VoucherCode));
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportDuplicateInFile,
                rowNumber: conflictRow.RowNumber);
        }

        await _importStore.BulkInsertStagingAsync(mutations, cancellationToken);
        var processedCount = request.StartProcessedCount + mutations.Count;
        if (processedCount > job.ExpectedCount)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportCountMismatch);
        }

        await _repository.ApplyProgressAsync(
            request.JobId,
            processedCount,
            false,
            request.ProcessedAt,
            cancellationToken);

        return new StageVoucherPoolImportBatchResult(
            mutations.Count,
            processedCount);
    }
}
