using Core.Abstractions;
using Core.Entities.Constants;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class RecordVoucherPoolGenerationFailureCommandHandler
    : IRequestHandler<RecordVoucherPoolGenerationFailureCommand>
{
    private readonly IVoucherPoolProvisioningRepository _repository;

    public RecordVoucherPoolGenerationFailureCommandHandler(
        IVoucherPoolProvisioningRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        RecordVoucherPoolGenerationFailureCommand request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetJobAsync(request.JobId, cancellationToken);
        if (job is null || job.Status == VoucherPoolProvisioningJobStatuses.Completed)
        {
            return;
        }

        var shouldRetry =
            request.Retriable &&
            job.AttemptCount < VoucherPoolGenerationLimits.MaxAttempts;
        await _repository.RecordFailureAsync(
            request.JobId,
            shouldRetry
                ? VoucherPoolProvisioningJobStatuses.Pending
                : VoucherPoolProvisioningJobStatuses.Failed,
            request.ErrorCode,
            request.ErrorDetails,
            shouldRetry ? null : request.FailedAt,
            cancellationToken);
    }
}
