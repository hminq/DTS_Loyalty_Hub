using Quartz;
using Microsoft.Extensions.Configuration;

namespace Scheduler.Options;

public sealed class TierExpirationScheduleOptions
{
    private const string CronKey = "TIER_EXPIRATION_CRON";
    private const string TimeZoneKey = "TIER_EXPIRATION_TIME_ZONE";
    private const string BatchSizeKey = "TIER_EXPIRATION_BATCH_SIZE";
    private const int MaximumBatchSize = 1_000;

    public string Cron { get; init; } = string.Empty;

    public string TimeZone { get; init; } = string.Empty;

    public int BatchSize { get; init; }

    public static TierExpirationScheduleOptions FromConfiguration(IConfiguration configuration)
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

        var timeZone = configuration[TimeZoneKey];
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            throw new InvalidOperationException(
                $"Missing required configuration value: {TimeZoneKey}");
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new InvalidOperationException(
                $"Configuration '{TimeZoneKey}' is not a valid system time zone.",
                exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new InvalidOperationException(
                $"Configuration '{TimeZoneKey}' is not a valid system time zone.",
                exception);
        }

        var batchSizeValue = configuration[BatchSizeKey];
        if (!int.TryParse(batchSizeValue, out var batchSize) ||
            batchSize <= 0 ||
            batchSize > MaximumBatchSize)
        {
            throw new InvalidOperationException(
                $"Configuration '{BatchSizeKey}' must be an integer between 1 and {MaximumBatchSize}.");
        }

        return new TierExpirationScheduleOptions
        {
            Cron = cron,
            TimeZone = timeZone,
            BatchSize = batchSize,
        };
    }
}
