using Quartz;
using Microsoft.Extensions.Configuration;

namespace Scheduler.Options;

public sealed class CampaignSessionLifecycleScheduleOptions
{
    private const string CronKey = "CAMPAIGN_SESSION_LIFECYCLE_CRON";
    private const string BatchSizeKey = "CAMPAIGN_SESSION_LIFECYCLE_BATCH_SIZE";
    private const int MaximumBatchSize = 1_000;

    public string Cron { get; init; } = string.Empty;

    public int BatchSize { get; init; }

    public static CampaignSessionLifecycleScheduleOptions FromConfiguration(IConfiguration configuration)
    {
        var cron = configuration[CronKey];
        if (string.IsNullOrWhiteSpace(cron))
        {
            throw new InvalidOperationException($"Missing required configuration value: {CronKey}");
        }

        if (!CronExpression.IsValidExpression(cron))
        {
            throw new InvalidOperationException(
                $"Configuration '{CronKey}' is not a valid Quartz cron expression.");
        }

        var batchSizeValue = configuration[BatchSizeKey];
        if (!int.TryParse(batchSizeValue, out var batchSize) ||
            batchSize <= 0 ||
            batchSize > MaximumBatchSize)
        {
            throw new InvalidOperationException(
                $"Configuration '{BatchSizeKey}' must be an integer between 1 and {MaximumBatchSize}.");
        }

        return new CampaignSessionLifecycleScheduleOptions
        {
            Cron = cron,
            BatchSize = batchSize,
        };
    }
}
