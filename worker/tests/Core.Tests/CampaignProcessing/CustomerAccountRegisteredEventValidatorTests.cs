using System.Text;
using System.Text.Json;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Core.Tests.CampaignProcessing;

public sealed class CustomerAccountRegisteredEventValidatorTests
{
    private static readonly Guid EventId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid CustomerId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid ReferrerCustomerId =
        Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly DateTime OccurredAt =
        new(2026, 7, 28, 8, 30, 0, DateTimeKind.Utc);

    private readonly CustomerAccountRegisteredEventValidator _validator = new();

    [Fact]
    public void Validate_ValidNormalEvent_ReturnsNormalizedEventAndHash()
    {
        var body = CreateBody(CustomerRegistrationSources.Normal, null);

        var result = Validate(body);

        result.EventId.Should().Be(EventId);
        result.EventType.Should().Be(EventTypeCodes.CustomerAccountRegistered);
        result.RoutingKey.Should().Be(EventRoutingKeys.CustomerAccountRegistered);
        result.OccurredAt.Should().Be(OccurredAt);
        result.UserId.Should().Be(UserId);
        result.CustomerId.Should().Be(CustomerId);
        result.Source.Should().Be(CustomerRegistrationSources.Normal);
        result.ReferrerCustomerId.Should().BeNull();
        result.PayloadHash.Should().MatchRegex("^[0-9a-f]{64}$");
        result.NormalizedPayload.Should().Contain("\"referrerCustomerId\":null");
    }

    [Fact]
    public void Validate_ValidReferralEvent_ReturnsReferrerIdentity()
    {
        var body = CreateBody(
            CustomerRegistrationSources.Referral,
            ReferrerCustomerId);

        var result = Validate(body);

        result.Source.Should().Be(CustomerRegistrationSources.Referral);
        result.ReferrerCustomerId.Should().Be(ReferrerCustomerId);
    }

    [Fact]
    public void Validate_EquivalentPropertyOrder_ProducesSameNormalizedPayloadAndHash()
    {
        var first = JsonSerializer.SerializeToUtf8Bytes(new
        {
            eventId = EventId,
            eventType = EventTypeCodes.CustomerAccountRegistered,
            routingKey = EventRoutingKeys.CustomerAccountRegistered,
            occurredAt = OccurredAt,
            data = new
            {
                userId = UserId,
                customerId = CustomerId,
                source = CustomerRegistrationSources.Normal,
                referrerCustomerId = (Guid?)null
            }
        });
        var second = JsonSerializer.SerializeToUtf8Bytes(new
        {
            data = new
            {
                referrerCustomerId = (Guid?)null,
                source = CustomerRegistrationSources.Normal,
                customerId = CustomerId,
                userId = UserId
            },
            occurredAt = OccurredAt,
            routingKey = EventRoutingKeys.CustomerAccountRegistered,
            eventType = EventTypeCodes.CustomerAccountRegistered,
            eventId = EventId
        });

        var firstResult = Validate(first);
        var secondResult = Validate(second);

        secondResult.NormalizedPayload.Should().Be(firstResult.NormalizedPayload);
        secondResult.PayloadHash.Should().Be(firstResult.PayloadHash);
    }

    [Fact]
    public void Validate_SubMicrosecondOccurredAt_TruncatesToPostgreSqlPrecision()
    {
        var occurredAt = new DateTime(
            new DateTime(2026, 7, 28, 8, 30, 0, DateTimeKind.Utc).Ticks + 7,
            DateTimeKind.Utc);
        var body = CreateBody(
            CustomerRegistrationSources.Normal,
            null,
            occurredAt: occurredAt);

        var result = Validate(body);

        result.OccurredAt.Ticks.Should().Be(occurredAt.Ticks - 7);
        result.NormalizedPayload.Should().Contain("2026-07-28T08:30:00Z");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("[]")]
    public void Validate_InvalidBody_ThrowsBodyInvalid(string body)
    {
        Action act = () => Validate(Encoding.UTF8.GetBytes(body));

        AssertError(act, CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Fact]
    public void Validate_UnknownEnvelopeProperty_ThrowsBodyInvalid()
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(new
        {
            eventId = EventId,
            eventType = EventTypeCodes.CustomerAccountRegistered,
            routingKey = EventRoutingKeys.CustomerAccountRegistered,
            occurredAt = OccurredAt,
            data = ValidData(),
            unexpected = true
        });

        Action act = () => Validate(body);

        AssertError(act, CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Fact]
    public void Validate_UnknownDataProperty_ThrowsPayloadInvalid()
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(new
        {
            eventId = EventId,
            eventType = EventTypeCodes.CustomerAccountRegistered,
            routingKey = EventRoutingKeys.CustomerAccountRegistered,
            occurredAt = OccurredAt,
            data = new
            {
                userId = UserId,
                customerId = CustomerId,
                source = CustomerRegistrationSources.Normal,
                referrerCustomerId = (Guid?)null,
                unexpected = true
            }
        });

        Action act = () => Validate(body);

        AssertError(act, CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Validate_EmptyEventId_ThrowsEventIdRequired()
    {
        var body = CreateBody(CustomerRegistrationSources.Normal, null, Guid.Empty);

        Action act = () => _validator.Validate(
            body,
            Guid.Empty.ToString(),
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered);

        AssertError(act, CampaignProcessingErrorCodes.EventIdRequired);
    }

    [Fact]
    public void Validate_DifferentMetadataEventId_ThrowsEventIdMismatch()
    {
        var body = CreateBody(CustomerRegistrationSources.Normal, null);

        Action act = () => _validator.Validate(
            body,
            Guid.NewGuid().ToString(),
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered);

        AssertError(act, CampaignProcessingErrorCodes.EventIdMismatch);
    }

    [Fact]
    public void Validate_DifferentMetadataType_ThrowsEventTypeMismatch()
    {
        var body = CreateBody(CustomerRegistrationSources.Normal, null);

        Action act = () => _validator.Validate(
            body,
            EventId.ToString(),
            "OTHER_EVENT",
            EventRoutingKeys.CustomerAccountRegistered);

        AssertError(act, CampaignProcessingErrorCodes.EventTypeMismatch);
    }

    [Fact]
    public void Validate_UnsupportedEventType_ThrowsEventTypeUnsupported()
    {
        var body = CreateBody(
            CustomerRegistrationSources.Normal,
            null,
            eventType: "OTHER_EVENT");

        Action act = () => _validator.Validate(
            body,
            EventId.ToString(),
            "OTHER_EVENT",
            EventRoutingKeys.CustomerAccountRegistered);

        AssertError(act, CampaignProcessingErrorCodes.EventTypeUnsupported);
    }

    [Fact]
    public void Validate_DifferentDeliveryRoutingKey_ThrowsRoutingKeyMismatch()
    {
        var body = CreateBody(CustomerRegistrationSources.Normal, null);

        Action act = () => _validator.Validate(
            body,
            EventId.ToString(),
            EventTypeCodes.CustomerAccountRegistered,
            "other.routing.key");

        AssertError(act, CampaignProcessingErrorCodes.EventRoutingKeyMismatch);
    }

    [Fact]
    public void Validate_NonUtcOccurredAt_ThrowsOccurredAtInvalid()
    {
        var body = CreateBody(
            CustomerRegistrationSources.Normal,
            null,
            occurredAt: DateTime.SpecifyKind(OccurredAt, DateTimeKind.Unspecified));

        Action act = () => Validate(body);

        AssertError(act, CampaignProcessingErrorCodes.EventOccurredAtInvalid);
    }

    [Fact]
    public void Validate_UnknownSource_ThrowsSourceInvalid()
    {
        var body = CreateBody("OTHER", null);

        Action act = () => Validate(body);

        AssertError(act, CampaignProcessingErrorCodes.EventSourceInvalid);
    }

    [Fact]
    public void Validate_NormalWithReferrer_ThrowsReferrerInvalid()
    {
        var body = CreateBody(
            CustomerRegistrationSources.Normal,
            ReferrerCustomerId);

        Action act = () => Validate(body);

        AssertError(act, CampaignProcessingErrorCodes.EventReferrerInvalid);
    }

    [Fact]
    public void Validate_ReferralWithoutReferrer_ThrowsReferrerInvalid()
    {
        var body = CreateBody(CustomerRegistrationSources.Referral, null);

        Action act = () => Validate(body);

        AssertError(act, CampaignProcessingErrorCodes.EventReferrerInvalid);
    }

    [Fact]
    public void Validate_SelfReferral_ThrowsReferrerInvalid()
    {
        var body = CreateBody(
            CustomerRegistrationSources.Referral,
            CustomerId);

        Action act = () => Validate(body);

        AssertError(act, CampaignProcessingErrorCodes.EventReferrerInvalid);
    }

    private static byte[] CreateBody(
        string source,
        Guid? referrerCustomerId,
        Guid? eventId = null,
        string? eventType = null,
        DateTime? occurredAt = null)
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            new OutgoingEvent<CustomerAccountRegisteredData>(
                eventId ?? EventId,
                eventType ?? EventTypeCodes.CustomerAccountRegistered,
                EventRoutingKeys.CustomerAccountRegistered,
                occurredAt ?? OccurredAt,
                new CustomerAccountRegisteredData(
                    UserId,
                    CustomerId,
                    source,
                    referrerCustomerId)),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static object ValidData()
    {
        return new
        {
            userId = UserId,
            customerId = CustomerId,
            source = CustomerRegistrationSources.Normal,
            referrerCustomerId = (Guid?)null
        };
    }

    private Core.Entities.Campaigns.ValidatedCustomerAccountRegisteredEvent Validate(
        ReadOnlyMemory<byte> body)
    {
        return _validator.Validate(
            body,
            EventId.ToString(),
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered);
    }

    private static void AssertError(Action act, string expectedErrorCode)
    {
        act.Should()
            .Throw<CampaignEventValidationException>()
            .Which.ErrorCode.Should()
            .Be(expectedErrorCode);
    }
}
