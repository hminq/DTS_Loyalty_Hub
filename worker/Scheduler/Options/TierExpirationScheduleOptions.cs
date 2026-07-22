using Quartz;

namespace Scheduler.Options;

public sealed class TierExpirationScheduleOptions
{
    public const string SectionName = "Scheduler:TierExpiration";

    public string Cron { get; init; } = string.Empty;

    public string TimeZone { get; init; } = string.Empty;

    public void Validate()
    {
        if (!CronExpression.IsValidExpression(Cron))
        {
            throw new InvalidOperationException(
                $"Configuration '{SectionName}:Cron' is not a valid Quartz cron expression.");
        }

        if (string.IsNullOrWhiteSpace(TimeZone))
        {
            throw new InvalidOperationException(
                $"Missing required configuration value: {SectionName}:TimeZone");
        }

        _ = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
    }
}
