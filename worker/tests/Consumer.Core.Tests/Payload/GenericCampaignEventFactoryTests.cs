using System.Globalization;
using System.Text.Json;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Entities.Definitions;
using Consumer.Core.Entities.Envelope;
using Consumer.Core.Exceptions;
using Consumer.Core.Handlers;
using Consumer.Core.Requests;
using Consumer.Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;

namespace Consumer.Core.Tests.Payload;

public sealed class GenericCampaignEventFactoryTests
{
    private readonly GenericCampaignEventFactory _factory = new();

    [Fact]
    public void Create_ValidPayloadWithAllTypes_ReturnsGenericValidatedCampaignEvent()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "age": 25,
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            new DateTime(2026, 7, 29, 8, 0, 0, DateTimeKind.Utc),
            payloadDoc.RootElement);

        var result = _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.EventType.Should().Be("CUSTOMER_ACCOUNT_REGISTERED");
        result.EventVersion.Should().Be(1);
        result.RoutingKey.Should().Be("customer.account.registered");
        result.PayloadHash.Should().HaveLength(64);

        result.PayloadValues["customerId"].Value.Should().Be(customerId.ToString("D"));
        result.PayloadValues["username"].Value.Should().Be("minh");
        result.PayloadValues["age"].Value.Should().Be(25m);
        result.PayloadValues["isActive"].Value.Should().Be(true);
    }

    [Fact]
    public void Create_AbsentOptionalField_ReturnsSuccessfullyAndOmitsFieldInCanonicalJson()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        // 'age' is optional in definition schema
        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            new DateTime(2026, 7, 29, 8, 0, 0, DateTimeKind.Utc),
            payloadDoc.RootElement);

        var result = _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        result.Should().NotBeNull();
        result.PayloadValues.ContainsKey("age").Should().BeFalse();
        result.NormalizedPayload.Should().NotContain("\"age\"");
    }

    [Fact]
    public void Create_MissingRequiredField_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var definition = CreateDefinition();

        // 'username' is required in definition schema
        var payloadJson = """
            {
              "age": 25,
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_ExplicitNullValue_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": null,
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_UnknownFieldInPayload_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "isActive": true,
              "unknownField": "test"
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_TypeMismatch_StringForNumber_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "age": "25",
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_TypeMismatch_StringForBoolean_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "isActive": "true"
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_TypeMismatch_NumberForString_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": 123,
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Theory]
    [InlineData("79228162514264337593543950335")]
    [InlineData("-79228162514264337593543950335")]
    public void Create_DecimalExtreme_ReturnsExactDecimal(string numberJson)
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();
        var payloadJson =
            $"{{\"customerId\":\"{customerId:D}\",\"username\":\"minh\",\"age\":{numberJson},\"isActive\":true}}";

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var result = _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        result.PayloadValues["age"].Value.Should().Be(
            decimal.Parse(numberJson, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("79228162514264337593543950336")]
    [InlineData("1e100")]
    public void Create_NumberOutsideDecimalRange_ThrowsEventPayloadInvalid(
        string numberJson)
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();
        var payloadJson =
            $"{{\"customerId\":\"{customerId:D}\",\"username\":\"minh\",\"age\":{numberJson},\"isActive\":true}}";

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_InvalidUuidFormat_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = """
            {
              "customerId": "not-a-valid-uuid",
              "username": "minh",
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_EmptyUuid_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{Guid.Empty}}",
              "username": "minh",
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_DuplicatePayloadField_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "username": "duplicate",
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_NestedObjectInPayload_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": { "nested": "value" },
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_ArrayInPayload_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": ["minh"],
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_CaseSensitiveFieldNameMismatch_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "CustomerId": "{{customerId}}",
              "Username": "minh",
              "IsActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "customer.account.registered");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Create_DeterministicCanonicalJsonAndHash_PropertyReorderingProducesIdenticalHash()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var occurredAt = new DateTime(2026, 7, 29, 8, 0, 0, DateTimeKind.Utc);
        var definition = CreateDefinition();

        var payloadJson1 = $$"""
            {
              "isActive": true,
              "age": 30,
              "username": "minh",
              "customerId": "{{customerId}}"
            }
            """;

        var payloadJson2 = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "age": 30,
              "isActive": true
            }
            """;

        using var doc1 = JsonDocument.Parse(payloadJson1);
        using var doc2 = JsonDocument.Parse(payloadJson2);

        var env1 = new RawEventEnvelope(eventId, "CUSTOMER_ACCOUNT_REGISTERED", 1, occurredAt, doc1.RootElement);
        var env2 = new RawEventEnvelope(eventId, "CUSTOMER_ACCOUNT_REGISTERED", 1, occurredAt, doc2.RootElement);

        var res1 = _factory.Create(env1, definition, "customer.account.registered");
        var res2 = _factory.Create(env2, definition, "customer.account.registered");

        res1.NormalizedPayload.Should().Be(res2.NormalizedPayload);
        res1.PayloadHash.Should().Be(res2.PayloadHash);
    }

    [Fact]
    public void Create_RoutingKeyMismatch_ThrowsEventRoutingKeyMismatch()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "isActive": true
            }
            """;

        using var payloadDoc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            DateTime.UtcNow,
            payloadDoc.RootElement);

        var act = () => _factory.Create(
            envelope,
            definition,
            "wrong.routing.key");

        act.Should().Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventRoutingKeyMismatch);
    }

    [Fact]
    public async Task Create_SevenFractionDigitTimestamp_DoesNotCauseEventIdCollisionOnRedelivery()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition();

        var occurredAt7Digits = new DateTime(2026, 7, 29, 8, 0, 0, DateTimeKind.Utc).AddTicks(1234567);
        var persistedOccurredAt = new DateTime(
            occurredAt7Digits.Ticks - occurredAt7Digits.Ticks % 10,
            DateTimeKind.Utc);

        var payloadJson = $$"""
            {
              "customerId": "{{customerId}}",
              "username": "minh",
              "isActive": true
            }
            """;

        using var doc = JsonDocument.Parse(payloadJson);
        var envelope = new RawEventEnvelope(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            1,
            occurredAt7Digits,
            doc.RootElement);

        var campaignEvent = _factory.Create(
            envelope,
            definition,
            "customer.account.registered");
        var persistedState = new EventPreparationState(
            eventId,
            "CUSTOMER_ACCOUNT_REGISTERED",
            definition.EventTypeVersionId,
            1,
            "customer.account.registered",
            persistedOccurredAt,
            campaignEvent.PayloadHash,
            EventProcessingStatuses.Completed,
            Array.Empty<PreparedCampaignTarget>());
        var store = new Mock<ICampaignEventPreparationStore>();
        store
            .Setup(x => x.TryInsertEventAsync(
                campaignEvent,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        store
            .Setup(x => x.GetEventStateAsync(
                eventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedState);
        var handler = new PrepareCampaignEventCommandHandler(
            store.Object,
            TimeProvider.System);

        var result = await handler.Handle(
            new PrepareCampaignEventCommand(campaignEvent),
            CancellationToken.None);

        campaignEvent.OccurredAt.Should().Be(persistedOccurredAt);
        campaignEvent.NormalizedPayload.Should()
            .Contain("\"occurredAt\":\"2026-07-29T08:00:00.123456Z\"");
        result.Created.Should().BeFalse();
        result.Status.Should().Be(EventProcessingStatuses.Completed);
    }

    private static PublishedEventDefinition CreateDefinition()
    {
        var schema = new EventPayloadSchema(
            Fields:
            [
                new EventPayloadFieldSchema("customerId", "STRING", "UUID", true, false),
                new EventPayloadFieldSchema("username", "STRING", null, true, true),
                new EventPayloadFieldSchema("age", "NUMBER", null, false, true),
                new EventPayloadFieldSchema("isActive", "BOOLEAN", null, true, true)
            ],
            Targets:
            [
                new EventTargetSchema("customer", "CUSTOMER", "customerId")
            ]);

        return new PublishedEventDefinition(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CUSTOMER_ACCOUNT_REGISTERED",
            "customer.account.registered",
            1,
            "PUBLISHED",
            DateTime.UtcNow,
            schema);
    }
}
