using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class CompleteVoucherPoolImportCommandHandler
    : IRequestHandler<CompleteVoucherPoolImportCommand>
{
    private readonly IVoucherPoolProvisioningRepository _repository;
    private readonly IVoucherPoolImportStore _importStore;

    public CompleteVoucherPoolImportCommandHandler(
        IVoucherPoolProvisioningRepository repository,
        IVoucherPoolImportStore importStore)
    {
        _repository = repository;
        _importStore = importStore;
    }

    public async Task Handle(
        CompleteVoucherPoolImportCommand request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetJobAsync(request.JobId, cancellationToken);
        if (job is null ||
            job.JobType != VoucherPoolProvisioningJobTypes.Imported ||
            job.Status != VoucherPoolProvisioningJobStatuses.Processing)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportStateInvalid);
        }

        var stagedCount = await _importStore.CountStagedRowsAsync(
            request.JobId,
            cancellationToken);
        if (job.ProcessedCount != job.ExpectedCount ||
            stagedCount != job.ExpectedCount)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportCountMismatch);
        }

        if (await _importStore.FindGlobalConflictAsync(
                request.JobId,
                cancellationToken) is not null)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportCodeAlreadyExists);
        }

        await _importStore.PromoteAsync(
            request.JobId,
            request.CompletedAt,
            cancellationToken);
    }
}
