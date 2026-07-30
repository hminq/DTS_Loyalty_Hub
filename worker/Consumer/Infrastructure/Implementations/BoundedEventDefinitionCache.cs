using System.Collections.Concurrent;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Definitions;
using Consumer.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Consumer.Infrastructure.Implementations;

public sealed class BoundedEventDefinitionCache : IEventDefinitionProvider
{
    private sealed record CacheEntry(
        PublishedEventDefinition Definition,
        DateTime ExpiresAt);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EventDefinitionCacheOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly object _entryLock = new();

    private readonly ConcurrentDictionary<(string EventType, int EventVersion), Lazy<Task<PublishedEventDefinition?>>> _cache = new();
    private readonly ConcurrentDictionary<(string EventType, int EventVersion), CacheEntry> _entries = new();

    public BoundedEventDefinitionCache(
        IServiceScopeFactory scopeFactory,
        EventDefinitionCacheOptions options,
        TimeProvider? timeProvider = null)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<PublishedEventDefinition?> GetDefinitionAsync(
        string eventType,
        int eventVersion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventType) || eventVersion <= 0)
        {
            return null;
        }

        var key = (eventType, eventVersion);
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        if (_entries.TryGetValue(key, out var existing))
        {
            if (existing.ExpiresAt > now)
            {
                return existing.Definition;
            }

            _entries.TryRemove(key, out _);
            _cache.TryRemove(key, out _);
        }

        var lazyTask = _cache.GetOrAdd(
            key,
            k => new Lazy<Task<PublishedEventDefinition?>>(
                () => LoadFromDbAsync(k.EventType, k.EventVersion, CancellationToken.None)));

        try
        {
            var definition = await lazyTask.Value.WaitAsync(cancellationToken);
            if (definition is null)
            {
                _cache.TryRemove(key, out _);
                return null;
            }

            lock (_entryLock)
            {
                var entry = new CacheEntry(
                    definition,
                    _timeProvider.GetUtcNow().UtcDateTime.AddSeconds(_options.TtlSeconds));
                _entries[key] = entry;
                EnsureCapacity();
            }

            return definition;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            _cache.TryRemove(key, out _);
            throw;
        }
    }

    private async Task<PublishedEventDefinition?> LoadFromDbAsync(
        string eventType,
        int eventVersion,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IEventDefinitionStore>();
        return await store.FetchDefinitionAsync(eventType, eventVersion, cancellationToken);
    }

    private void EnsureCapacity()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        if (_entries.Count < _options.MaxEntries)
        {
            return;
        }

        foreach (var pair in _entries)
        {
            if (pair.Value.ExpiresAt <= now)
            {
                _entries.TryRemove(pair.Key, out _);
                _cache.TryRemove(pair.Key, out _);
            }
        }

        if (_entries.Count < _options.MaxEntries)
        {
            return;
        }

        foreach (var key in _entries
                     .OrderBy(pair => pair.Value.ExpiresAt)
                     .ThenBy(pair => pair.Key.EventType, StringComparer.Ordinal)
                     .ThenBy(pair => pair.Key.EventVersion)
                     .Select(pair => pair.Key)
                     .Take(Math.Max(0, _entries.Count - _options.MaxEntries))
                     .ToArray())
        {
            _entries.TryRemove(key, out _);
            _cache.TryRemove(key, out _);
        }
    }
}
