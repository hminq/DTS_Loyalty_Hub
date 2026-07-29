namespace Scheduler.Core.Entities.Constants;

public static class OutboxDispatchConstants
{
    public const int IntervalSeconds = 1;

    public const int BatchSize = 100;

    public const int MaxAttempts = 3;

    public const int BaseRetrySeconds = 5;
}
