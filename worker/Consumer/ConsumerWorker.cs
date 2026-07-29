using System.Diagnostics;
using Consumer.Core.Abstractions;
using Consumer.Core.Services;

namespace Consumer;

public sealed class ConsumerWorker(
    ICampaignEventDeliverySource deliverySource,
    CampaignEventDeliveryProcessor deliveryProcessor,
    ILogger<ConsumerWorker> logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return deliverySource.RunAsync(
            async (delivery, cancellationToken) =>
            {
                var startedAt = Stopwatch.GetTimestamp();
                var result = await deliveryProcessor.ProcessAsync(
                    delivery,
                    cancellationToken);

                logger.LogInformation(
                    "Campaign event delivery processed. EventId={EventId} " +
                    "EventType={EventType} RoutingKey={RoutingKey} " +
                    "Redelivered={Redelivered} PinnedCampaignCount={PinnedCampaignCount} " +
                    "CompletedCampaignCount={CompletedCampaignCount} " +
                    "SkippedCampaignCount={SkippedCampaignCount} " +
                    "FailedCampaignCount={FailedCampaignCount} " +
                    "BrokerOutcome={BrokerOutcome} ErrorCode={ErrorCode} ElapsedMs={ElapsedMs}",
                    result.EventId,
                    delivery.MessageType,
                    delivery.RoutingKey,
                    delivery.Redelivered,
                    result.PinnedCampaignCount,
                    result.CompletedCampaignCount,
                    result.SkippedCampaignCount,
                    result.FailedCampaignCount,
                    result.Disposition,
                    result.ErrorCode,
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);

                return result;
            },
            stoppingToken);
    }
}
