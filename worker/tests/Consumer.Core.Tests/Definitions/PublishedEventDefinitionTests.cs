using Consumer.Core.Entities.Definitions;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Consumer.Core.Tests.Definitions;

public sealed class PublishedEventDefinitionTests
{
    [Fact]
    public void Constructor_DuplicateFields_ThrowsArgumentException()
    {
        var schema = new EventPayloadSchema(
            [
                new EventPayloadFieldSchema("customerId", "STRING", "UUID", true, false),
                new EventPayloadFieldSchema("customerId", "STRING", "UUID", true, false)
            ],
            [new EventTargetSchema("CUSTOMER", "CUSTOMER", "customerId")]);

        var act = () => CreateDefinition(schema);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_DuplicateTargets_ThrowsArgumentException()
    {
        var schema = new EventPayloadSchema(
            [new EventPayloadFieldSchema("customerId", "STRING", "UUID", true, false)],
            [
                new EventTargetSchema("CUSTOMER", "CUSTOMER", "customerId"),
                new EventTargetSchema("CUSTOMER", "CUSTOMER", "customerId")
            ]);

        var act = () => CreateDefinition(schema);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_TargetIdFieldMustBeRequiredStringUuid()
    {
        var schema = new EventPayloadSchema(
            [new EventPayloadFieldSchema("customerId", "STRING", null, true, false)],
            [new EventTargetSchema("CUSTOMER", "CUSTOMER", "customerId")]);

        var act = () => CreateDefinition(schema);

        act.Should().Throw<ArgumentException>();
    }

    private static PublishedEventDefinition CreateDefinition(EventPayloadSchema schema)
    {
        return new PublishedEventDefinition(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CUSTOMER_REFERRAL_SUCCEEDED",
            "customer.referral.succeeded",
            1,
            "PUBLISHED",
            DateTime.UtcNow,
            schema);
    }
}
