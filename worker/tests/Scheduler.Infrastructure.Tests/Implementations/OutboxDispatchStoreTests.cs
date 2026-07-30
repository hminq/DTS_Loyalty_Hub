using FluentAssertions;
using Messaging.Contracts.Outbox;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;
using Scheduler.Infrastructure.Implementations;

namespace Scheduler.Infrastructure.Tests.Implementations;

public sealed class OutboxDispatchStoreTests
{
    [Fact]
    public async Task GetDispatchableAsync_ProjectVersionIdentityAndStoredPayload()
    {
        await using var dbContext = CreateDbContext();
        var now = new DateTime(2026, 7, 30, 8, 0, 0, DateTimeKind.Utc);
        var eventId = Guid.NewGuid();
        var eventTypeVersionId = Guid.NewGuid();
        const string storedPayload = """
            {"eventId":"8e41a1cf-27dd-4cb0-a5bb-260c38e8f3d7","eventType":"CUSTOMER_REFERRAL_SUCCEEDED","eventVersion":1,"occurredAt":"2026-07-30T08:00:00Z","payload":{"referrerCustomerId":"33fbf026-c61a-4021-bdf0-ee1c5b9b2c67","referredCustomerId":"f1528296-b7f4-4396-89e0-80de13dd7dfd","referredUsername":"nguyenminh9"}}
            """;

        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            EventId = eventId,
            EventType = "CUSTOMER_REFERRAL_SUCCEEDED",
            RoutingKey = "customer.referral.succeeded",
            EventTypeVersionId = eventTypeVersionId,
            EventVersion = 1,
            Payload = storedPayload,
            Status = OutboxMessageStatuses.Pending,
            AttemptCount = 1,
            NextAttemptAt = now.AddSeconds(-1),
            OccurredAt = now,
            CreatedAt = now,
            PublishedAt = null
        });
        await dbContext.SaveChangesAsync();

        var store = new OutboxDispatchStore(dbContext);

        var result = await store.GetDispatchableAsync(eventId, now, maxAttempts: 3, CancellationToken.None);

        result.Should().NotBeNull();
        result!.EventId.Should().Be(eventId);
        result.EventTypeVersionId.Should().Be(eventTypeVersionId);
        result.EventType.Should().Be("CUSTOMER_REFERRAL_SUCCEEDED");
        result.EventVersion.Should().Be(1);
        result.RoutingKey.Should().Be("customer.referral.succeeded");
        result.Payload.Should().Be(storedPayload);
        result.AttemptCount.Should().Be(1);
    }

    private static LoyaltyHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LoyaltyHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LoyaltyHubDbContext(options);
    }
}
