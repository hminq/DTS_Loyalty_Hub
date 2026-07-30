using System.Text.Json;
using Campaign.Contracts.Constants;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Campaigns;
using Core.UseCases.Campaigns.Results;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Core.Tests.UseCases.Campaigns;

public sealed class CampaignConfigurationServiceTests
{
    private readonly CampaignConfigurationService _service = new();

    [Fact]
    public void ParseCondition_StringField_ReturnsCanonicalDsl()
    {
        var result = _service.ParseCondition(
            RegistrationDefinition(),
            """{"all":[{"operator":"CONTAINS","value":"minh","field":"username"}]}""");

        result.Should().Be(
            """{"all":[{"field":"username","operator":"CONTAINS","value":"minh"}]}""");
    }

    [Fact]
    public void ParseCondition_EmptyAll_ReturnsCanonicalMatchAll()
    {
        var result = _service.ParseCondition(
            RegistrationDefinition(),
            """{"all":[]}""");

        result.Should().Be("""{"all":[]}""");
    }

    [Theory]
    [InlineData("""{"all":[{"field":"Username","operator":"CONTAINS","value":"minh"}]}""")]
    [InlineData("""{"all":[{"field":"unknown","operator":"CONTAINS","value":"minh"}]}""")]
    [InlineData("""{"all":[{"field":"customerId","operator":"EQUALS","value":"2fffc3ab-9773-43f7-957e-e8acb40ab996"}]}""")]
    [InlineData("""{"all":[{"field":"username","operator":"CONTAINS","value":"minh"}],"unexpected":true}""")]
    [InlineData("""{"all":[{"field":"username","operator":"CONTAINS","value":"minh","unexpected":true}]}""")]
    [InlineData("""{"all":[{"all":[{"field":"username","operator":"CONTAINS","value":"minh"}]}]}""")]
    public void ParseCondition_UnsupportedOrMalformedContract_Throws(string json)
    {
        var action = () => _service.ParseCondition(RegistrationDefinition(), json);

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("CAMPAIGN_CONDITION_INVALID");
    }

    [Fact]
    public void ParseCondition_UuidFormattedString_AcceptsValidUuid()
    {
        var result = _service.ParseCondition(
            RegistrationDefinition(),
            """{"all":[{"field":"registeredCustomerId","operator":"EQUALS","value":"2fffc3ab-9773-43f7-957e-e8acb40ab996"}]}""");

        result.Should().Contain("registeredCustomerId");
    }

    [Fact]
    public void ParseCondition_UuidFormattedString_RejectsInvalidUuid()
    {
        var action = () => _service.ParseCondition(
            RegistrationDefinition(),
            """{"all":[{"field":"registeredCustomerId","operator":"EQUALS","value":"not-a-uuid"}]}""");

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("CAMPAIGN_CONDITION_INVALID");
    }

    [Fact]
    public void ParseCondition_NumberField_AcceptsComparison()
    {
        var result = _service.ParseCondition(
            RegistrationDefinition(),
            """{"all":[{"field":"amount","operator":"GTE","value":50}]}""");

        result.Should().Be("""{"all":[{"field":"amount","operator":"GTE","value":50}]}""");
    }

    [Fact]
    public void ParseCondition_BooleanField_AcceptsEquals()
    {
        var result = _service.ParseCondition(
            RegistrationDefinition(),
            """{"all":[{"field":"isReferral","operator":"EQUALS","value":true}]}""");

        result.Should().Be("""{"all":[{"field":"isReferral","operator":"EQUALS","value":true}]}""");
    }

    [Fact]
    public void ParseAction_IssuePointForRegisteredCustomer_ReturnsCanonicalEnvelope()
    {
        var result = _service.ParseAction(
            RegistrationDefinition(),
            " issue_point ",
            """
            {
              "parameters": { "amount": 50 },
              "target": { "selector": "REGISTERED_CUSTOMER" }
            }
            """);

        result.ActionType.Should().Be(ActionTypes.IssuePoint);
        result.ActionConfig.Should().Be(
            """{"target":{"selector":"REGISTERED_CUSTOMER"},"parameters":{"amount":50}}""");
    }

    [Fact]
    public void GetActionUniquenessKey_NormalizesCanonicalActionConfig()
    {
        var normalized = _service.GetActionUniquenessKey(
            RegistrationDefinition(),
            " issue_point ",
            """{"parameters":{"amount":50},"target":{"selector":"REGISTERED_CUSTOMER"}}""");
        var canonical = _service.GetActionUniquenessKey(
            RegistrationDefinition(),
            "ISSUE_POINT",
            """{"target":{"selector":"REGISTERED_CUSTOMER"},"parameters":{"amount":50}}""");

        normalized.Should().Be(canonical);
    }

    [Theory]
    [InlineData("""{"target":{"selector":"REGISTERED_CUSTOMER"},"parameters":{"amount":50.001}}""")]
    [InlineData("""{"target":{"selector":"REGISTERED_CUSTOMER"},"parameters":{"amount":50},"unexpected":true}""")]
    [InlineData("""{"target":{"selector":"REGISTERED_CUSTOMER"},"parameters":{"amount":50,"percentage":10}}""")]
    [InlineData("""{"target":{"selector":"UNKNOWN"},"parameters":{"amount":50}}""")]
    public void ParseAction_UnsupportedOrMalformedContract_Throws(string json)
    {
        var action = () => _service.ParseAction(
            RegistrationDefinition(),
            ActionTypes.IssuePoint,
            json);

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().BeOneOf("CAMPAIGN_ACTION_CONFIG_INVALID", "CAMPAIGN_ACTION_TARGET_INVALID");
    }

    private static CampaignEventDefinitionResult RegistrationDefinition()
    {
        var schema = new EventPayloadSchema(
            [
                new EventPayloadFieldSchema("username", EventPayloadDataTypes.String, null, true, true),
                new EventPayloadFieldSchema("registeredCustomerId", EventPayloadDataTypes.String, "UUID", true, true),
                new EventPayloadFieldSchema("customerId", EventPayloadDataTypes.String, "UUID", true, false),
                new EventPayloadFieldSchema("amount", EventPayloadDataTypes.Number, null, false, true),
                new EventPayloadFieldSchema("isReferral", EventPayloadDataTypes.Boolean, null, false, true)
            ],
            [
                new EventTargetSchema("REGISTERED_CUSTOMER", CampaignTargetKinds.Customer, "registeredCustomerId")
            ]);

        return new CampaignEventDefinitionResult(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            EventTypeCodes.CustomerAccountRegistered,
            "customer.account.registered",
            "Customer account registered",
            EventDefinitionStatuses.Active,
            1,
            EventDefinitionVersionStatuses.Published,
            JsonSerializer.Serialize(schema, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
