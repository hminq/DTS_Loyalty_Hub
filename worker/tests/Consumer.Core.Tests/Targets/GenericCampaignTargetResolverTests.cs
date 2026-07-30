using Campaign.Contracts.Constants;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Entities.Definitions;
using Consumer.Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Consumer.Core.Tests.Targets;

public sealed class GenericCampaignTargetResolverTests
{
    private readonly GenericCampaignTargetResolver _resolver = new();

    [Fact]
    public void Resolve_ExactCustomerSelector_ReturnsConfiguredPayloadUuid()
    {
        var customerId = Guid.NewGuid();
        var campaignEvent = EventWithTargets(
            [
                new EventTargetSchema("registeredCustomer", EventTargetKinds.Customer, "registeredCustomerId")
            ],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal)
            {
                ["registeredCustomerId"] = UuidValue("registeredCustomerId", customerId)
            });

        var result = _resolver.Resolve(
            campaignEvent,
            "registeredCustomer",
            CampaignTargetKinds.Customer);

        result.Status.Should().Be(CampaignTargetResolutionStatuses.Resolved);
        result.TargetKind.Should().Be(CampaignTargetKinds.Customer);
        result.TargetId.Should().Be(customerId);
    }

    [Fact]
    public void Resolve_TwoArbitrarySelectors_ReturnDifferentPayloadIds()
    {
        var firstCustomerId = Guid.NewGuid();
        var secondCustomerId = Guid.NewGuid();
        var campaignEvent = EventWithTargets(
            [
                new EventTargetSchema("buyer", EventTargetKinds.Customer, "buyerId"),
                new EventTargetSchema("recipient", EventTargetKinds.Customer, "recipientId")
            ],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal)
            {
                ["buyerId"] = UuidValue("buyerId", firstCustomerId),
                ["recipientId"] = UuidValue("recipientId", secondCustomerId)
            });

        _resolver.Resolve(campaignEvent, "buyer", CampaignTargetKinds.Customer)
            .TargetId.Should().Be(firstCustomerId);
        _resolver.Resolve(campaignEvent, "recipient", CampaignTargetKinds.Customer)
            .TargetId.Should().Be(secondCustomerId);
    }

    [Fact]
    public void Resolve_CaseDistinctSelectors_ResolveIndependently()
    {
        var lowerCustomerId = Guid.NewGuid();
        var upperCustomerId = Guid.NewGuid();
        var campaignEvent = EventWithTargets(
            [
                new EventTargetSchema("customer", EventTargetKinds.Customer, "lowerCustomerId"),
                new EventTargetSchema("Customer", EventTargetKinds.Customer, "upperCustomerId")
            ],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal)
            {
                ["lowerCustomerId"] = UuidValue("lowerCustomerId", lowerCustomerId),
                ["upperCustomerId"] = UuidValue("upperCustomerId", upperCustomerId)
            });

        _resolver.Resolve(campaignEvent, "customer", CampaignTargetKinds.Customer)
            .TargetId.Should().Be(lowerCustomerId);
        _resolver.Resolve(campaignEvent, "Customer", CampaignTargetKinds.Customer)
            .TargetId.Should().Be(upperCustomerId);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("Customer")]
    [InlineData("")]
    [InlineData(" customer")]
    [InlineData("customer ")]
    public void Resolve_UnknownOrNonExactSelector_ReturnsInvalidConfiguration(string selector)
    {
        var campaignEvent = EventWithTargets(
            [new EventTargetSchema("customer", EventTargetKinds.Customer, "customerId")],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal)
            {
                ["customerId"] = UuidValue("customerId", Guid.NewGuid())
            });

        var result = _resolver.Resolve(
            campaignEvent,
            selector,
            CampaignTargetKinds.Customer);

        result.Status.Should().Be(CampaignTargetResolutionStatuses.Unsupported);
        result.ErrorCode.Should().Be(CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }

    [Fact]
    public void Resolve_TargetKindMismatch_ReturnsInvalidConfiguration()
    {
        var campaignEvent = EventWithTargets(
            [new EventTargetSchema("customer", EventTargetKinds.Customer, "customerId")],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal)
            {
                ["customerId"] = UuidValue("customerId", Guid.NewGuid())
            });

        var result = _resolver.Resolve(
            campaignEvent,
            "customer",
            "ACCOUNT");

        result.Status.Should().Be(CampaignTargetResolutionStatuses.Unsupported);
        result.ErrorCode.Should().Be(CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }

    [Fact]
    public void Resolve_SelectorUnknownToConsumerCode_StillResolvesFromSchema()
    {
        var customerId = Guid.NewGuid();
        var campaignEvent = EventWithTargets(
            [new EventTargetSchema("externalPublisherCustomer", EventTargetKinds.Customer, "externalCustomerId")],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal)
            {
                ["externalCustomerId"] = UuidValue("externalCustomerId", customerId)
            });

        var result = _resolver.Resolve(
            campaignEvent,
            "externalPublisherCustomer",
            CampaignTargetKinds.Customer);

        result.Status.Should().Be(CampaignTargetResolutionStatuses.Resolved);
        result.TargetId.Should().Be(customerId);
    }

    [Fact]
    public void Resolve_DoesNotInferFromFirstSchemaTargetOrActionType()
    {
        var firstCustomerId = Guid.NewGuid();
        var campaignEvent = EventWithTargets(
            [
                new EventTargetSchema("first", EventTargetKinds.Customer, "firstId"),
                new EventTargetSchema("second", EventTargetKinds.Customer, "secondId")
            ],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal)
            {
                ["firstId"] = UuidValue("firstId", firstCustomerId),
                ["secondId"] = UuidValue("secondId", Guid.NewGuid())
            });

        var result = _resolver.Resolve(
            campaignEvent,
            ActionTypes.IssuePoint,
            CampaignTargetKinds.Customer);

        result.Status.Should().Be(CampaignTargetResolutionStatuses.Unsupported);
        result.TargetId.Should().BeNull();
    }

    [Fact]
    public void Resolve_MissingTargetId_ReturnsInvalidEventPayload()
    {
        var campaignEvent = EventWithTargets(
            [new EventTargetSchema("customer", EventTargetKinds.Customer, "customerId")],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal));

        var result = _resolver.Resolve(
            campaignEvent,
            "customer",
            CampaignTargetKinds.Customer);

        result.Status.Should().Be(CampaignTargetResolutionStatuses.InvalidEvent);
        result.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Theory]
    [MemberData(nameof(InvalidTargetValues))]
    public void Resolve_InvalidTargetIdValue_ReturnsInvalidEventPayload(ValidatedPayloadValue targetValue)
    {
        var campaignEvent = EventWithTargets(
            [new EventTargetSchema("customer", EventTargetKinds.Customer, "customerId")],
            new Dictionary<string, ValidatedPayloadValue>(StringComparer.Ordinal)
            {
                ["customerId"] = targetValue
            });

        var result = _resolver.Resolve(
            campaignEvent,
            "customer",
            CampaignTargetKinds.Customer);

        result.Status.Should().Be(CampaignTargetResolutionStatuses.InvalidEvent);
        result.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    public static TheoryData<ValidatedPayloadValue> InvalidTargetValues()
    {
        return new TheoryData<ValidatedPayloadValue>
        {
            new("customerId", EventPayloadDataTypes.Number, null, 10m),
            new("customerId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, "not-a-uuid"),
            new("customerId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.Empty.ToString("D"))
        };
    }

    private static ValidatedPayloadValue UuidValue(string code, Guid value)
    {
        return new ValidatedPayloadValue(
            code,
            EventPayloadDataTypes.String,
            EventPayloadFormats.Uuid,
            value.ToString("D"));
    }

    private static GenericValidatedCampaignEvent EventWithTargets(
        IReadOnlyList<EventTargetSchema> targets,
        IReadOnlyDictionary<string, ValidatedPayloadValue> values)
    {
        var fields = targets
            .Select(target => new EventPayloadFieldSchema(
                target.IdField,
                EventPayloadDataTypes.String,
                EventPayloadFormats.Uuid,
                true,
                false))
            .ToArray();

        var definition = new PublishedEventDefinition(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "GENERIC_TEST_EVENT",
            "generic.test",
            1,
            "PUBLISHED",
            DateTime.UtcNow,
            new EventPayloadSchema(fields, targets));

        return new GenericValidatedCampaignEvent(
            Guid.NewGuid(),
            definition.EventTypeId,
            definition.EventTypeVersionId,
            definition.EventTypeCode,
            definition.Version,
            definition.RoutingKey,
            DateTime.UtcNow,
            definition,
            values,
            "{}",
            new string('0', 64));
    }
}
