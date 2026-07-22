using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Scheduler.Options;

namespace Scheduler.Jobs;

[DisallowConcurrentExecution]
public sealed class ProcessExpiredCustomerTiersJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TierExpirationScheduleOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessExpiredCustomerTiersJob> _logger;

    public ProcessExpiredCustomerTiersJob(
        IServiceScopeFactory scopeFactory,
        TierExpirationScheduleOptions options,
        TimeProvider timeProvider,
        ILogger<ProcessExpiredCustomerTiersJob> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var processedAt = _timeProvider.GetUtcNow().UtcDateTime;
        var totalProcessed = 0;

        while (true)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            ProcessExpiredCustomerTierBatchResult result;
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                result = await sender.Send(
                    new ProcessExpiredCustomerTierBatchCommand(
                        processedAt,
                        _options.BatchSize),
                    context.CancellationToken);
            }

            totalProcessed += result.ProcessedCount;

            if (result.SelectedCount < _options.BatchSize)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Processed {ProcessedCount} customers with expired tiers.",
            totalProcessed);
    }
}
