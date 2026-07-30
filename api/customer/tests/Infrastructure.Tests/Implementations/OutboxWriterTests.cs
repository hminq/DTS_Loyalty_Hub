using System.Text.Json;
using Core.UseCases.Events.Models;
using FluentAssertions;
using Infrastructure.Implementations;
using Messaging.Contracts.Events;
using Messaging.Contracts.Outbox;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Tests.Implementations;

public sealed class OutboxWriterTests
{
    [Fact]
    public void Add_CustomerAccountRegisteredEvent_TracksVersionedEnvelope()
    {
        using var dbContext = CreateDbContext();
        var writer = new OutboxWriter(dbContext);
        var occurredAt = new DateTime(2026, 7, 28, 8, 30, 0, DateTimeKind.Utc);
        var reference = new PublishedEventVersionReference(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            1,
            EventRoutingKeys.CustomerAccountRegistered);
        var envelope = new EventEnvelope<CustomerAccountRegisteredPayload>(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            1,
            occurredAt,
            new CustomerAccountRegisteredPayload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "nguyenminh9",
                "Nguyen Minh"));

        writer.Add(new VersionedOutboxEvent<CustomerAccountRegisteredPayload>(reference, envelope));

        var outboxMessage = dbContext.ChangeTracker
            .Entries<OutboxMessage>()
            .Should().ContainSingle()
            .Which.Entity;
        using var payload = JsonDocument.Parse(outboxMessage.Payload);
        var root = payload.RootElement;

        outboxMessage.EventId.Should().Be(envelope.EventId);
        outboxMessage.EventType.Should().Be(reference.EventType);
        outboxMessage.RoutingKey.Should().Be(reference.RoutingKey);
        outboxMessage.EventTypeVersionId.Should().Be(reference.EventTypeVersionId);
        outboxMessage.EventVersion.Should().Be(reference.EventVersion);
        outboxMessage.Status.Should().Be(OutboxMessageStatuses.Pending);
        outboxMessage.AttemptCount.Should().Be(0);
        outboxMessage.NextAttemptAt.Should().Be(occurredAt);
        outboxMessage.OccurredAt.Should().Be(occurredAt);
        outboxMessage.CreatedAt.Should().Be(occurredAt);
        outboxMessage.PublishedAt.Should().BeNull();
        outboxMessage.LastErrorCode.Should().BeNull();
        outboxMessage.LastError.Should().BeNull();

        root.EnumerateObject().Select(property => property.Name).Should().Equal(
            "eventId",
            "eventType",
            "eventVersion",
            "occurredAt",
            "payload");
        root.GetProperty("eventId").GetGuid().Should().Be(envelope.EventId);
        root.GetProperty("eventType").GetString().Should().Be(envelope.EventType);
        root.GetProperty("eventVersion").GetInt32().Should().Be(envelope.EventVersion);
        root.GetProperty("occurredAt").GetString().Should().EndWith("Z");
        root.GetProperty("occurredAt").GetDateTime().Should().Be(occurredAt);
        root.GetProperty("payload").GetProperty("userId").GetGuid().Should().Be(envelope.Payload.UserId);
        root.GetProperty("payload").GetProperty("customerId").GetGuid().Should().Be(envelope.Payload.CustomerId);
        root.GetProperty("payload").GetProperty("username").GetString().Should().Be("nguyenminh9");
        root.GetProperty("payload").GetProperty("fullName").GetString().Should().Be("Nguyen Minh");
        root.TryGetProperty("data", out _).Should().BeFalse();
        root.TryGetProperty("routingKey", out _).Should().BeFalse();
        root.GetProperty("payload").TryGetProperty("source", out _).Should().BeFalse();
    }

    [Fact]
    public void Add_CustomerReferralSucceededEvent_TracksVersionedEnvelope()
    {
        using var dbContext = CreateDbContext();
        var writer = new OutboxWriter(dbContext);
        var occurredAt = new DateTime(2026, 7, 28, 8, 30, 0, DateTimeKind.Utc);
        var reference = new PublishedEventVersionReference(
            Guid.NewGuid(),
            EventTypeCodes.CustomerReferralSucceeded,
            1,
            EventRoutingKeys.CustomerReferralSucceeded);
        var envelope = new EventEnvelope<CustomerReferralSucceededPayload>(
            Guid.NewGuid(),
            EventTypeCodes.CustomerReferralSucceeded,
            1,
            occurredAt,
            new CustomerReferralSucceededPayload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "nguyenminh9"));

        writer.Add(new VersionedOutboxEvent<CustomerReferralSucceededPayload>(reference, envelope));

        var outboxMessage = dbContext.ChangeTracker
            .Entries<OutboxMessage>()
            .Should().ContainSingle()
            .Which.Entity;
        using var payload = JsonDocument.Parse(outboxMessage.Payload);
        var root = payload.RootElement;

        outboxMessage.EventId.Should().Be(envelope.EventId);
        outboxMessage.EventTypeVersionId.Should().Be(reference.EventTypeVersionId);
        outboxMessage.EventVersion.Should().Be(reference.EventVersion);
        outboxMessage.RoutingKey.Should().Be(reference.RoutingKey);
        root.GetProperty("payload").GetProperty("referrerCustomerId").GetGuid()
            .Should().Be(envelope.Payload.ReferrerCustomerId);
        root.GetProperty("payload").GetProperty("referredCustomerId").GetGuid()
            .Should().Be(envelope.Payload.ReferredCustomerId);
        root.GetProperty("payload").GetProperty("referredUsername").GetString()
            .Should().Be("nguyenminh9");
        root.GetProperty("payload").TryGetProperty("userId", out _).Should().BeFalse();
        root.GetProperty("payload").TryGetProperty("source", out _).Should().BeFalse();
    }

    [Fact]
    public void Add_EventTypeMismatch_ThrowsBeforeTrackingEntity()
    {
        using var dbContext = CreateDbContext();
        var writer = new OutboxWriter(dbContext);
        var reference = new PublishedEventVersionReference(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            1,
            EventRoutingKeys.CustomerAccountRegistered);
        var envelope = new EventEnvelope<CustomerReferralSucceededPayload>(
            Guid.NewGuid(),
            EventTypeCodes.CustomerReferralSucceeded,
            1,
            new DateTime(2026, 7, 28, 8, 30, 0, DateTimeKind.Utc),
            new CustomerReferralSucceededPayload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "nguyenminh9"));

        var act = () => writer.Add(new VersionedOutboxEvent<CustomerReferralSucceededPayload>(reference, envelope));

        act.Should().Throw<InvalidOperationException>();
        dbContext.ChangeTracker.Entries<OutboxMessage>().Should().BeEmpty();
    }

    [Fact]
    public void Add_EventVersionMismatch_ThrowsBeforeTrackingEntity()
    {
        using var dbContext = CreateDbContext();
        var writer = new OutboxWriter(dbContext);
        var reference = new PublishedEventVersionReference(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            2,
            EventRoutingKeys.CustomerAccountRegistered);
        var envelope = new EventEnvelope<CustomerAccountRegisteredPayload>(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            1,
            new DateTime(2026, 7, 28, 8, 30, 0, DateTimeKind.Utc),
            new CustomerAccountRegisteredPayload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "nguyenminh9",
                "Nguyen Minh"));

        var act = () => writer.Add(new VersionedOutboxEvent<CustomerAccountRegisteredPayload>(reference, envelope));

        act.Should().Throw<InvalidOperationException>();
        dbContext.ChangeTracker.Entries<OutboxMessage>().Should().BeEmpty();
    }

    [Fact]
    public void EventEnvelope_InvalidMetadata_Throws()
    {
        var utcNow = new DateTime(2026, 7, 28, 8, 30, 0, DateTimeKind.Utc);
        var payload = new CustomerAccountRegisteredPayload(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "nguyenminh9",
            "Nguyen Minh");

        var emptyId = () => new EventEnvelope<CustomerAccountRegisteredPayload>(
            Guid.Empty,
            EventTypeCodes.CustomerAccountRegistered,
            1,
            utcNow,
            payload);
        var blankType = () => new EventEnvelope<CustomerAccountRegisteredPayload>(
            Guid.NewGuid(),
            " ",
            1,
            utcNow,
            payload);
        var nonPositiveVersion = () => new EventEnvelope<CustomerAccountRegisteredPayload>(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            0,
            utcNow,
            payload);
        var localTime = () => new EventEnvelope<CustomerAccountRegisteredPayload>(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            1,
            new DateTime(2026, 7, 28, 8, 30, 0, DateTimeKind.Local),
            payload);
        var nullPayload = () => new EventEnvelope<CustomerAccountRegisteredPayload?>(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            1,
            utcNow,
            null);

        emptyId.Should().Throw<ArgumentException>();
        blankType.Should().Throw<ArgumentException>();
        nonPositiveVersion.Should().Throw<ArgumentOutOfRangeException>();
        localTime.Should().Throw<ArgumentException>();
        nullPayload.Should().Throw<ArgumentNullException>();
    }

    private static LoyaltyHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LoyaltyHubDbContext>()
            .UseNpgsql("Host=localhost;Database=test;Username=test;Password=test")
            .Options;

        return new LoyaltyHubDbContext(options);
    }
}
