using System.Text.Json;
using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Scheduler.Options;

namespace Scheduler.Jobs;

[DisallowConcurrentExecution]
public sealed class ProcessVoucherPoolProvisioningJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IVoucherPoolGenerationFailureClassifier _failureClassifier;
    private readonly VoucherPoolProvisioningScheduleOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessVoucherPoolProvisioningJob> _logger;

    public ProcessVoucherPoolProvisioningJob(
        IServiceScopeFactory scopeFactory,
        IVoucherPoolGenerationFailureClassifier failureClassifier,
        VoucherPoolProvisioningScheduleOptions options,
        TimeProvider timeProvider,
        ILogger<ProcessVoucherPoolProvisioningJob> logger)
    {
        _scopeFactory = scopeFactory;
        _failureClassifier = failureClassifier;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        StartOrResumeVoucherPoolGenerationResult startResult;
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            startResult = await sender.Send(
                new StartOrResumeVoucherPoolGenerationCommand(
                    _timeProvider.GetUtcNow().UtcDateTime),
                context.CancellationToken);
        }

        if (!startResult.HasWork || startResult.JobId is null)
        {
            if (startResult.Failed)
            {
                _logger.LogWarning(
                    "Voucher pool provisioning job {JobId} failed state validation.",
                    startResult.JobId);
            }

            return;
        }

        var jobId = startResult.JobId.Value;
        var totalGenerated = 0;
        var processedCount = startResult.ProcessedCount;

        try
        {
            if (startResult.JobType == VoucherPoolProvisioningJobTypes.Imported)
            {
                await ProcessImportedAsync(
                    jobId,
                    startResult.ImportFileKey!,
                    startResult.ProcessedCount,
                    current => processedCount = current,
                    context.CancellationToken);
                _logger.LogInformation(
                    "Completed imported voucher pool provisioning job {JobId}.",
                    jobId);
                return;
            }

            while (true)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                GenerateVoucherPoolBatchResult batchResult;
                await using (var scope = _scopeFactory.CreateAsyncScope())
                {
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                    batchResult = await sender.Send(
                        new GenerateVoucherPoolBatchCommand(
                            jobId,
                            _options.BatchSize,
                            _timeProvider.GetUtcNow().UtcDateTime),
                        context.CancellationToken);
                }

                totalGenerated += batchResult.GeneratedCount;
                processedCount = batchResult.ProcessedCount;
                if (batchResult.IsCompleted)
                {
                    _logger.LogInformation(
                        "Completed voucher pool provisioning job {JobId}. Generated {GeneratedCount} rows in this execution.",
                        jobId,
                        totalGenerated);
                    return;
                }
            }
        }
        catch (OperationCanceledException)
            when (context.CancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Voucher pool provisioning job {JobId} was interrupted and will resume later.",
                jobId);
            throw;
        }
        catch (Exception exception)
        {
            var failure = _failureClassifier.Classify(exception);
            if (startResult.JobType == VoucherPoolProvisioningJobTypes.Imported)
            {
                failure = failure.ErrorCode switch
                {
                    VoucherPoolGenerationErrorCodes.DatabaseError =>
                        failure with
                        {
                            ErrorCode = VoucherPoolGenerationErrorCodes.ImportDatabaseError
                        },
                    VoucherPoolGenerationErrorCodes.UnexpectedError =>
                        failure with
                        {
                            ErrorCode = VoucherPoolGenerationErrorCodes.ImportUnexpectedError
                        },
                    _ => failure
                };
            }
            var errorDetails = JsonSerializer.Serialize(new
            {
                startResult.ExpectedCount,
                ProcessedCount = processedCount,
                RowNumber = (exception as VoucherPoolImportException)?.RowNumber
            });

            await using var scope = _scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(
                new RecordVoucherPoolGenerationFailureCommand(
                    jobId,
                    failure.ErrorCode,
                    errorDetails,
                    failure.Retriable,
                    _timeProvider.GetUtcNow().UtcDateTime),
                CancellationToken.None);

            _logger.LogError(
                exception,
                "Voucher pool provisioning job {JobId} failed with code {ErrorCode}.",
                jobId,
                failure.ErrorCode);
        }
    }

    private async Task ProcessImportedAsync(
        Guid jobId,
        string objectKey,
        int committedCount,
        Action<int> onProgress,
        CancellationToken cancellationToken)
    {
        await using var streamScope = _scopeFactory.CreateAsyncScope();
        var reader = streamScope.ServiceProvider
            .GetRequiredService<IVoucherPoolImportFileReader>();
        var buffer = new List<VoucherPoolImportRawRow>(_options.BatchSize);
        var parsedCount = 0;
        var processedCount = committedCount;

        await foreach (var row in reader.ReadAsync(objectKey, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            parsedCount++;
            if (parsedCount <= committedCount)
            {
                continue;
            }

            buffer.Add(row);
            if (buffer.Count < _options.BatchSize)
            {
                continue;
            }

            processedCount = await StageImportBatchAsync(
                jobId,
                processedCount,
                buffer,
                cancellationToken);
            onProgress(processedCount);
            buffer.Clear();
        }

        if (buffer.Count > 0)
        {
            processedCount = await StageImportBatchAsync(
                jobId,
                processedCount,
                buffer,
                cancellationToken);
            onProgress(processedCount);
        }

        if (parsedCount != processedCount)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportCountMismatch);
        }

        await using var completionScope = _scopeFactory.CreateAsyncScope();
        var sender = completionScope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(
            new CompleteVoucherPoolImportCommand(
                jobId,
                _timeProvider.GetUtcNow().UtcDateTime),
            cancellationToken);
    }

    private async Task<int> StageImportBatchAsync(
        Guid jobId,
        int processedCount,
        IReadOnlyCollection<VoucherPoolImportRawRow> rows,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var result = await sender.Send(
            new StageVoucherPoolImportBatchCommand(
                jobId,
                processedCount,
                rows.ToArray(),
                _timeProvider.GetUtcNow().UtcDateTime),
            cancellationToken);
        return result.ProcessedCount;
    }
}
