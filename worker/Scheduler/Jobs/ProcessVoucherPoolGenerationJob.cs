using System.Text.Json;
using Core.Abstractions;
using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Scheduler.Options;

namespace Scheduler.Jobs;

[DisallowConcurrentExecution]
public sealed class ProcessVoucherPoolGenerationJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IVoucherPoolGenerationFailureClassifier _failureClassifier;
    private readonly VoucherPoolGenerationScheduleOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessVoucherPoolGenerationJob> _logger;

    public ProcessVoucherPoolGenerationJob(
        IServiceScopeFactory scopeFactory,
        IVoucherPoolGenerationFailureClassifier failureClassifier,
        VoucherPoolGenerationScheduleOptions options,
        TimeProvider timeProvider,
        ILogger<ProcessVoucherPoolGenerationJob> logger)
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
            var errorDetails = JsonSerializer.Serialize(new
            {
                startResult.ExpectedCount,
                ProcessedCount = processedCount
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
}
