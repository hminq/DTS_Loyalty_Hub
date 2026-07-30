using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Definitions;
using Consumer.Infrastructure.Implementations;
using Consumer.Infrastructure.Options;
using FluentAssertions;
using Messaging.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Consumer.Infrastructure.Tests.Definitions;

public sealed class BoundedEventDefinitionCacheTests
{
    private readonly Mock<IEventDefinitionStore> _storeMock = new();
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly EventDefinitionCacheOptions _options = new(maxEntries: 5, ttlSeconds: 10);

    public BoundedEventDefinitionCacheTests()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEventDefinitionStore)))
            .Returns(_storeMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        _scopeFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(scopeMock.Object);
    }

    [Fact]
    public async Task GetDefinitionAsync_ValidDefinition_CachesAndReturnsDefinition()
    {
        var definition = CreateDefinition("CUSTOMER_ACCOUNT_REGISTERED", 1);
        _storeMock
            .Setup(s => s.FetchDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        var cache = new BoundedEventDefinitionCache(_scopeFactoryMock.Object, _options);

        var result1 = await cache.GetDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1);
        var result2 = await cache.GetDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1);

        result1.Should().Be(definition);
        result2.Should().Be(definition);

        _storeMock.Verify(
            s => s.FetchDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDefinitionAsync_MissingOrUnpublishedDefinition_DoesNotCacheNegativeResult()
    {
        _storeMock
            .Setup(s => s.FetchDefinitionAsync("UNKNOWN_EVENT", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublishedEventDefinition?)null);

        var cache = new BoundedEventDefinitionCache(_scopeFactoryMock.Object, _options);

        var result1 = await cache.GetDefinitionAsync("UNKNOWN_EVENT", 1);
        var result2 = await cache.GetDefinitionAsync("UNKNOWN_EVENT", 1);

        result1.Should().BeNull();
        result2.Should().BeNull();

        _storeMock.Verify(
            s => s.FetchDefinitionAsync("UNKNOWN_EVENT", 1, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetDefinitionAsync_ConcurrentRequests_CoalescesIntoSingleStoreCall()
    {
        var definition = CreateDefinition("CUSTOMER_ACCOUNT_REGISTERED", 1);
        var tcs = new TaskCompletionSource<PublishedEventDefinition?>();

        _storeMock
            .Setup(s => s.FetchDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1, It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cache = new BoundedEventDefinitionCache(_scopeFactoryMock.Object, _options);

        var task1 = cache.GetDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1);
        var task2 = cache.GetDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1);
        var task3 = cache.GetDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1);

        tcs.SetResult(definition);

        var results = await Task.WhenAll(task1, task2, task3);

        results[0].Should().Be(definition);
        results[1].Should().Be(definition);
        results[2].Should().Be(definition);

        _storeMock.Verify(
            s => s.FetchDefinitionAsync("CUSTOMER_ACCOUNT_REGISTERED", 1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDefinitionAsync_StoreFailure_DoesNotCacheFailureAndRemovesTask()
    {
        _storeMock
            .SetupSequence(s => s.FetchDefinitionAsync("FAIL_EVENT", 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"))
            .ReturnsAsync(CreateDefinition("FAIL_EVENT", 1));

        var cache = new BoundedEventDefinitionCache(_scopeFactoryMock.Object, _options);

        var act = () => cache.GetDefinitionAsync("FAIL_EVENT", 1);
        await act.Should().ThrowAsync<InvalidOperationException>();

        var result2 = await cache.GetDefinitionAsync("FAIL_EVENT", 1);
        result2.Should().NotBeNull();

        _storeMock.Verify(
            s => s.FetchDefinitionAsync("FAIL_EVENT", 1, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetDefinitionAsync_ExpiredEntry_ReloadsDefinition()
    {
        var first = CreateDefinition("EXPIRING_EVENT", 1);
        var second = CreateDefinition("EXPIRING_EVENT", 1);
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 7, 30, 0, 0, 0, TimeSpan.Zero));

        _storeMock
            .SetupSequence(s => s.FetchDefinitionAsync("EXPIRING_EVENT", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(first)
            .ReturnsAsync(second);

        var cache = new BoundedEventDefinitionCache(
            _scopeFactoryMock.Object,
            new EventDefinitionCacheOptions(maxEntries: 5, ttlSeconds: 10),
            timeProvider);

        var result1 = await cache.GetDefinitionAsync("EXPIRING_EVENT", 1);
        timeProvider.Advance(TimeSpan.FromSeconds(11));
        var result2 = await cache.GetDefinitionAsync("EXPIRING_EVENT", 1);

        result1.Should().Be(first);
        result2.Should().Be(second);
        _storeMock.Verify(
            s => s.FetchDefinitionAsync("EXPIRING_EVENT", 1, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetDefinitionAsync_ExceedsCapacity_EvictsOldestEntry()
    {
        _storeMock
            .Setup(s => s.FetchDefinitionAsync(It.IsAny<string>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string eventType, int version, CancellationToken _) =>
                CreateDefinition(eventType, version));

        var cache = new BoundedEventDefinitionCache(
            _scopeFactoryMock.Object,
            new EventDefinitionCacheOptions(maxEntries: 2, ttlSeconds: 60));

        await cache.GetDefinitionAsync("EVENT_A", 1);
        await cache.GetDefinitionAsync("EVENT_B", 1);
        await cache.GetDefinitionAsync("EVENT_C", 1);
        await cache.GetDefinitionAsync("EVENT_A", 1);

        _storeMock.Verify(
            s => s.FetchDefinitionAsync("EVENT_A", 1, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    private static PublishedEventDefinition CreateDefinition(string code, int version)
    {
        var schema = new EventPayloadSchema(
            Fields:
            [
                new EventPayloadFieldSchema("customerId", "STRING", "UUID", true, false),
                new EventPayloadFieldSchema("username", "STRING", null, true, true)
            ],
            Targets: [new EventTargetSchema("REGISTERED_CUSTOMER", "CUSTOMER", "customerId")]);

        return new PublishedEventDefinition(
            Guid.NewGuid(),
            Guid.NewGuid(),
            code,
            "test.routing.key",
            version,
            "PUBLISHED",
            DateTime.UtcNow,
            schema);
    }

    private sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan interval)
        {
            _now = _now.Add(interval);
        }
    }
}
