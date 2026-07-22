using Core.Requests;
using MediatR;
using Quartz;

namespace Scheduler.Jobs;

[DisallowConcurrentExecution]
public sealed class ProcessExpiredCustomerTiersJob : IJob
{
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessExpiredCustomerTiersJob> _logger;

    public ProcessExpiredCustomerTiersJob(
        ISender sender,
        TimeProvider timeProvider,
        ILogger<ProcessExpiredCustomerTiersJob> logger)
    {
        _sender = sender;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var processedCount = await _sender.Send(
            new ProcessExpiredCustomerTiersCommand(_timeProvider.GetUtcNow().UtcDateTime),
            context.CancellationToken);

        _logger.LogInformation(
            "Processed {ProcessedCount} customers with expired tiers.",
            processedCount);
    }
}
