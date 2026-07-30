using System.Text;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Services;
using FluentAssertions;

namespace Consumer.Core.Tests.Envelope;

public sealed class VersionedEnvelopeParserTests
{
    private readonly VersionedEnvelopeParser _parser = new();

    [Fact]
    public void Parse_ValidFivePropertyEnvelope_ReturnsParsedRawEnvelope()
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {
                "username": "minh"
              }
            }
            """;

        var result = _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.EventType.Should().Be("CUSTOMER_ACCOUNT_REGISTERED");
        result.EventVersion.Should().Be(1);
        result.OccurredAt.Should().Be(new DateTime(2026, 7, 29, 8, 0, 0, DateTimeKind.Utc));
        result.PayloadElement.GetProperty("username").GetString().Should().Be("minh");
    }

    [Fact]
    public void Parse_EmptyBody_ThrowsEventBodyInvalid()
    {
        var act = () => _parser.Parse(
            Array.Empty<byte>(),
            Guid.NewGuid().ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Fact]
    public void Parse_MalformedJson_ThrowsEventBodyInvalid()
    {
        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes("{ malformed json }"),
            Guid.NewGuid().ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Fact]
    public void Parse_ExtraProperty_ThrowsEventBodyInvalid()
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {},
              "extraProperty": 123
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Fact]
    public void Parse_OldDataBody_ThrowsEventBodyInvalid()
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {},
              "data": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Fact]
    public void Parse_OldRoutingKeyInBody_ThrowsEventBodyInvalid()
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {},
              "routingKey": "customer.account.registered"
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Fact]
    public void Parse_DuplicateProperty_ThrowsEventBodyInvalid()
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {},
              "eventId": "{{eventId}}"
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Fact]
    public void Parse_EmptyEventId_ThrowsEventIdRequired()
    {
        var json = """
            {
              "eventId": "00000000-0000-0000-0000-000000000000",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            Guid.Empty.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventIdRequired);
    }

    [Theory]
    [InlineData("eventId")]
    [InlineData("eventType")]
    [InlineData("eventVersion")]
    [InlineData("occurredAt")]
    [InlineData("payload")]
    public void Parse_MissingRequiredProperty_ThrowsEventBodyInvalid(string missingProperty)
    {
        var eventId = Guid.NewGuid();
        var properties = new Dictionary<string, string>
        {
            ["eventId"] = $"""
                "eventId": "{eventId}"
                """,
            ["eventType"] = """
                "eventType": "CUSTOMER_ACCOUNT_REGISTERED"
                """,
            ["eventVersion"] = """
                "eventVersion": 1
                """,
            ["occurredAt"] = """
                "occurredAt": "2026-07-29T08:00:00Z"
                """,
            ["payload"] = """
                "payload": {}
                """
        };
        properties.Remove(missingProperty);
        var json = $"{{{string.Join(",", properties.Values)}}}";

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Parse_InvalidEventVersion_ThrowsEventBodyInvalid(int version)
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": {{version}},
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Theory]
    [InlineData("\"1\"")]
    [InlineData("1.5")]
    public void Parse_EventVersionCoercion_ThrowsEventBodyInvalid(string versionJson)
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": {{versionJson}},
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventBodyInvalid);
    }

    [Theory]
    [InlineData("2026-07-29T08:00:00")]
    [InlineData("2026-07-29T15:00:00+07:00")]
    public void Parse_OccurredAtWithoutUtcDesignator_ThrowsOccurredAtInvalid(string occurredAt)
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "{{occurredAt}}",
              "payload": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventOccurredAtInvalid);
    }

    [Fact]
    public void Parse_NonObjectPayload_ThrowsEventPayloadInvalid()
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": [1, 2, 3]
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
    }

    [Fact]
    public void Parse_AmqpMessageIdMismatch_ThrowsEventIdMismatch()
    {
        var eventId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            otherId.ToString(),
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventIdMismatch);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Parse_MissingAmqpMessageId_ThrowsEventIdRequired(string? messageId)
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            messageId,
            "CUSTOMER_ACCOUNT_REGISTERED");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventIdRequired);
    }

    [Fact]
    public void Parse_AmqpTypeMismatch_ThrowsEventTypeMismatch()
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            "WRONG_TYPE");

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventTypeMismatch);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Parse_MissingAmqpType_ThrowsEventTypeMismatch(string? messageType)
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "eventType": "CUSTOMER_ACCOUNT_REGISTERED",
              "eventVersion": 1,
              "occurredAt": "2026-07-29T08:00:00Z",
              "payload": {}
            }
            """;

        var act = () => _parser.Parse(
            Encoding.UTF8.GetBytes(json),
            eventId.ToString(),
            messageType);

        act.Should().Throw<EventEnvelopeParseException>()
            .Which.ErrorCode.Should().Be(CampaignProcessingErrorCodes.EventTypeMismatch);
    }
}
