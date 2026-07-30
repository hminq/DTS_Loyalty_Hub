using System.Text.Json;
using Consumer.Infrastructure.Implementations;
using FluentAssertions;
using Messaging.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Consumer.Infrastructure.Tests.Definitions;

public sealed class EfEventDefinitionStoreTests
{
    [Fact]
    public async Task FetchDefinitionAsync_PublishedExactVersion_ReturnsDefinition()
    {
        await using var dbContext = CreateDbContext();
        var eventType = AddEventType(dbContext, "CUSTOMER_REFERRAL_SUCCEEDED", "customer.referral.succeeded");
        var version = AddVersion(dbContext, eventType, version: 1, status: "PUBLISHED");
        await dbContext.SaveChangesAsync();

        var store = new EfEventDefinitionStore(dbContext);

        var result = await store.FetchDefinitionAsync("CUSTOMER_REFERRAL_SUCCEEDED", 1);

        result.Should().NotBeNull();
        result!.EventTypeVersionId.Should().Be(version.EventTypeVersionId);
        result.Version.Should().Be(1);
    }

    [Fact]
    public async Task FetchDefinitionAsync_RetiredExactVersion_ReturnsDefinition()
    {
        await using var dbContext = CreateDbContext();
        var eventType = AddEventType(dbContext, "RETIRED_EVENT", "retired.event");
        AddVersion(dbContext, eventType, version: 1, status: "RETIRED");
        await dbContext.SaveChangesAsync();

        var store = new EfEventDefinitionStore(dbContext);

        var result = await store.FetchDefinitionAsync("RETIRED_EVENT", 1);

        result.Should().NotBeNull();
        result!.Status.Should().Be("RETIRED");
    }

    [Theory]
    [InlineData("DRAFT", true)]
    [InlineData("PUBLISHED", false)]
    public async Task FetchDefinitionAsync_NotRuntimeResolvable_ReturnsNull(
        string status,
        bool hasPublishedAt)
    {
        await using var dbContext = CreateDbContext();
        var eventType = AddEventType(dbContext, "UNRESOLVABLE_EVENT", "unresolvable.event");
        AddVersion(dbContext, eventType, version: 1, status: status, hasPublishedAt: hasPublishedAt);
        await dbContext.SaveChangesAsync();

        var store = new EfEventDefinitionStore(dbContext);

        var result = await store.FetchDefinitionAsync("UNRESOLVABLE_EVENT", 1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task FetchDefinitionAsync_MissingVersion_DoesNotFallbackToExistingVersion()
    {
        await using var dbContext = CreateDbContext();
        var eventType = AddEventType(dbContext, "VERSIONED_EVENT", "versioned.event");
        AddVersion(dbContext, eventType, version: 1, status: "PUBLISHED");
        await dbContext.SaveChangesAsync();

        var store = new EfEventDefinitionStore(dbContext);

        var result = await store.FetchDefinitionAsync("VERSIONED_EVENT", 2);

        result.Should().BeNull();
    }

    [Fact]
    public async Task FetchDefinitionAsync_InvalidPublishedSchema_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var eventType = AddEventType(dbContext, "INVALID_SCHEMA_EVENT", "invalid.schema.event");
        AddVersion(
            dbContext,
            eventType,
            version: 1,
            status: "PUBLISHED",
            schema: new EventPayloadSchema(
                [
                    new EventPayloadFieldSchema("customerId", "STRING", "UUID", true, false),
                    new EventPayloadFieldSchema("customerId", "STRING", "UUID", true, false)
                ],
                [new EventTargetSchema("CUSTOMER", "CUSTOMER", "customerId")]));
        await dbContext.SaveChangesAsync();

        var store = new EfEventDefinitionStore(dbContext);

        var result = await store.FetchDefinitionAsync("INVALID_SCHEMA_EVENT", 1);

        result.Should().BeNull();
    }

    private static LoyaltyHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LoyaltyHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new LoyaltyHubDbContext(options);
    }

    private static EventType AddEventType(
        LoyaltyHubDbContext dbContext,
        string code,
        string routingKey)
    {
        var eventType = new EventType
        {
            EventTypeId = Guid.NewGuid(),
            Code = code,
            RoutingKey = routingKey,
            Name = code,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.EventTypes.Add(eventType);
        return eventType;
    }

    private static EventTypeVersion AddVersion(
        LoyaltyHubDbContext dbContext,
        EventType eventType,
        int version,
        string status,
        bool hasPublishedAt = true,
        EventPayloadSchema? schema = null)
    {
        var eventTypeVersion = new EventTypeVersion
        {
            EventTypeVersionId = Guid.NewGuid(),
            EventTypeId = eventType.EventTypeId,
            EventType = eventType,
            Version = version,
            PayloadSchema = JsonSerializer.Serialize(
                schema ?? ValidSchema(),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = hasPublishedAt ? DateTime.UtcNow : null
        };
        dbContext.EventTypeVersions.Add(eventTypeVersion);
        return eventTypeVersion;
    }

    private static EventPayloadSchema ValidSchema()
    {
        return new EventPayloadSchema(
            [
                new EventPayloadFieldSchema("customerId", "STRING", "UUID", true, false),
                new EventPayloadFieldSchema("source", "STRING", null, false, true)
            ],
            [new EventTargetSchema("CUSTOMER", "CUSTOMER", "customerId")]);
    }
}
