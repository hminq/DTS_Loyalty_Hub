using Microsoft.Extensions.Configuration;
using Quartz;

namespace Scheduler.Options;

public sealed class VoucherPoolProvisioningScheduleOptions
{
    private const string CronKey = "VOUCHER_POOL_PROVISIONING_CRON";
    private const string TimeZoneKey = "VOUCHER_POOL_PROVISIONING_TIME_ZONE";
    private const string BatchSizeKey = "VOUCHER_POOL_PROVISIONING_BATCH_SIZE";
    private const int MaximumBatchSize = 10_000;

    public string Cron { get; init; } = string.Empty;

    public string TimeZone { get; init; } = string.Empty;

    public int BatchSize { get; init; }

    public static VoucherPoolProvisioningScheduleOptions FromConfiguration(
        IConfiguration configuration)
    {
        var cron = configuration[CronKey];
        if (string.IsNullOrWhiteSpace(cron) ||
            !CronExpression.IsValidExpression(cron))
        {
            throw new InvalidOperationException(
                $"Configuration '{CronKey}' must be a valid Quartz cron expression.");
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
        catch (Exception exception)
            when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new InvalidOperationException(
                $"Configuration '{TimeZoneKey}' is not a valid system time zone.",
                exception);
        }

        if (!int.TryParse(configuration[BatchSizeKey], out var batchSize) ||
            batchSize <= 0 ||
            batchSize > MaximumBatchSize)
        {
            throw new InvalidOperationException(
                $"Configuration '{BatchSizeKey}' must be an integer between 1 and {MaximumBatchSize}.");
        }

        return new VoucherPoolProvisioningScheduleOptions
        {
            Cron = cron,
            TimeZone = timeZone,
            BatchSize = batchSize
        };
    }
}
