using Microsoft.Extensions.Configuration;

namespace Consumer.Infrastructure.Options;

public sealed class EventDefinitionCacheOptions
{
    public const int MaximumMaxEntries = 10_000;
    public const int MaximumTtlSeconds = 86_400;

    public EventDefinitionCacheOptions(int maxEntries, int ttlSeconds)
    {
        if (maxEntries is <= 0 or > MaximumMaxEntries)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxEntries),
                $"EVENT_DEFINITION_CACHE_MAX_ENTRIES must be between 1 and {MaximumMaxEntries}.");
        }

        if (ttlSeconds is <= 0 or > MaximumTtlSeconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ttlSeconds),
                $"EVENT_DEFINITION_CACHE_TTL_SECONDS must be between 1 and {MaximumTtlSeconds}.");
        }

        MaxEntries = maxEntries;
        TtlSeconds = ttlSeconds;
    }

    public int MaxEntries { get; }
    public int TtlSeconds { get; }

    public static EventDefinitionCacheOptions FromConfiguration(IConfiguration configuration)
    {
        var maxEntriesRaw = configuration["EVENT_DEFINITION_CACHE_MAX_ENTRIES"];
        if (string.IsNullOrWhiteSpace(maxEntriesRaw) ||
            !int.TryParse(maxEntriesRaw, out var maxEntries) ||
            maxEntries is <= 0 or > MaximumMaxEntries)
        {
            throw new InvalidOperationException(
                $"Missing or invalid required configuration value: EVENT_DEFINITION_CACHE_MAX_ENTRIES must be between 1 and {MaximumMaxEntries}.");
        }

        var ttlSecondsRaw = configuration["EVENT_DEFINITION_CACHE_TTL_SECONDS"];
        if (string.IsNullOrWhiteSpace(ttlSecondsRaw) ||
            !int.TryParse(ttlSecondsRaw, out var ttlSeconds) ||
            ttlSeconds is <= 0 or > MaximumTtlSeconds)
        {
            throw new InvalidOperationException(
                $"Missing or invalid required configuration value: EVENT_DEFINITION_CACHE_TTL_SECONDS must be between 1 and {MaximumTtlSeconds}.");
        }

        return new EventDefinitionCacheOptions(maxEntries, ttlSeconds);
    }
}
