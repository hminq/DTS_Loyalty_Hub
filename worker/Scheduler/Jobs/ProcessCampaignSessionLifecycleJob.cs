using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Scheduler.Options;

namespace Scheduler.Jobs;

[DisallowConcurrentExecution]
public sealed class ProcessCampaignSessionLifecycleJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CampaignSessionLifecycleScheduleOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessCampaignSessionLifecycleJob> _logger;

    public ProcessCampaignSessionLifecycleJob(
        IServiceScopeFactory scopeFactory,
        CampaignSessionLifecycleScheduleOptions options,
        TimeProvider timeProvider,
        ILogger<ProcessCampaignSessionLifecycleJob> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var processedAt = _timeProvider.GetUtcNow().UtcDateTime;
        int totalStartedSessions = 0;
        int totalEndedSessions = 0;
        int totalEndedCampaigns = 0;

        while (true)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            ProcessCampaignSessionLifecycleBatchResult result;
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                result = await sender.Send(
                    new ProcessCampaignSessionLifecycleBatchCommand(
                        processedAt,
                        _options.BatchSize),
                    context.CancellationToken);
            }

            totalStartedSessions += result.StartedCount;
            totalEndedSessions += result.EndedCount;

            if (!result.HasMore(_options.BatchSize))
            {
                break;
            }
        }

        while (true)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            EndEligibleCampaignsBatchResult result;
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                result = await sender.Send(
                    new EndEligibleCampaignsBatchCommand(
                        processedAt,
                        _options.BatchSize),
                    context.CancellationToken);
            }

            totalEndedCampaigns += result.EndedCount;

            if (!result.HasMore(_options.BatchSize))
            {
                break;
            }
        }

        _logger.LogInformation(
            "Campaign session lifecycle processing completed. Started: {StartedCount}, Ended Sessions: {EndedSessionsCount}, Ended Campaigns: {EndedCampaignsCount}.",
            totalStartedSessions,
            totalEndedSessions,
            totalEndedCampaigns);
    }
}
