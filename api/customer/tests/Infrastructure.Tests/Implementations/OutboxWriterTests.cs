using System.Text.Json;
using FluentAssertions;
using Infrastructure.Implementations;
using Messaging.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Tests.Implementations;

public sealed class OutboxWriterTests
{
    [Fact]
    public void Add_CustomerRegistrationEvent_PersistsCompleteSharedEnvelope()
    {
        var options = new DbContextOptionsBuilder<LoyaltyHubDbContext>()
            .UseNpgsql("Host=localhost;Database=test;Username=test;Password=test")
            .Options;
        using var dbContext = new LoyaltyHubDbContext(options);
        var writer = new OutboxWriter(dbContext);
        var outgoingEvent = new OutgoingEvent<CustomerAccountRegisteredData>(
            Guid.NewGuid(),
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered,
            new DateTime(2026, 7, 28, 8, 30, 0, DateTimeKind.Utc),
            new CustomerAccountRegisteredData(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CustomerRegistrationSources.Normal,
                null));

        writer.Add(outgoingEvent);

        var outboxMessage = dbContext.ChangeTracker
            .Entries<OutboxMessage>()
            .Should().ContainSingle()
            .Which.Entity;
        using var payload = JsonDocument.Parse(outboxMessage.Payload);
        var root = payload.RootElement;

        root.GetProperty("eventId").GetGuid().Should().Be(outgoingEvent.EventId);
        root.GetProperty("eventType").GetString().Should().Be(outgoingEvent.EventType);
        root.GetProperty("routingKey").GetString().Should().Be(outgoingEvent.RoutingKey);
        root.GetProperty("occurredAt").GetDateTime().Should().Be(outgoingEvent.OccurredAt);
        root.GetProperty("data").GetProperty("customerId").GetGuid()
            .Should().Be(outgoingEvent.Data.CustomerId);
        root.EnumerateObject().Select(property => property.Name).Should().BeEquivalentTo(
            "eventId",
            "eventType",
            "routingKey",
            "occurredAt",
            "data");
    }
}
