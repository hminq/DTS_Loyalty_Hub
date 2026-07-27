using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Scheduler.Jobs;

[DisallowConcurrentExecution]
public sealed class DispatchOutboxMessagesJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DispatchOutboxMessagesJob> _logger;

    public DispatchOutboxMessagesJob(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<DispatchOutboxMessagesJob> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        IReadOnlyList<Guid> eventIds;
        await using (var selectionScope = _scopeFactory.CreateAsyncScope())
        {
            var sender = selectionScope.ServiceProvider.GetRequiredService<ISender>();
            eventIds = await sender.Send(
                new GetDispatchableOutboxMessageIdsQuery(
                    _timeProvider.GetUtcNow().UtcDateTime),
                context.CancellationToken);
        }

        var published = 0;
        var retried = 0;
        var failed = 0;
        var skipped = 0;

        foreach (var eventId in eventIds)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                await using var publishScope = _scopeFactory.CreateAsyncScope();
                var sender = publishScope.ServiceProvider.GetRequiredService<ISender>();
                var result = await sender.Send(
                    new PublishOutboxMessageCommand(eventId),
                    context.CancellationToken);

                switch (result.Outcome)
                {
                    case OutboxDispatchOutcome.Published:
                        published++;
                        break;
                    case OutboxDispatchOutcome.Retried:
                        retried++;
                        break;
                    case OutboxDispatchOutcome.Failed:
                        failed++;
                        break;
                    case OutboxDispatchOutcome.Skipped:
                        skipped++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(result.Outcome),
                            result.Outcome,
                            "Unknown outbox dispatch outcome.");
                }
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failed++;
                _logger.LogError(
                    exception,
                    "Unexpected failure while dispatching outbox event {EventId}; continuing selected batch.",
                    eventId);
            }
        }

        _logger.LogInformation(
            "Outbox dispatch batch completed: selected={Selected}, published={Published}, retried={Retried}, failed={Failed}, skipped={Skipped}.",
            eventIds.Count,
            published,
            retried,
            failed,
            skipped);
    }
}
