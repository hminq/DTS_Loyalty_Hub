using System.Text.Json;
using Core.Exceptions;
using Core.UseCases.Campaigns;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Core.Tests.UseCases.Campaigns;

public sealed class CampaignConfigurationServiceTests
{
    private readonly CampaignConfigurationService _service = new();

    [Fact]
    public void ParseCondition_NormalSource_ReturnsCanonicalDsl()
    {
        var result = _service.ParseCondition(
            " customer_account_registered ",
            """{"all":[{"operator":"equals","value":" normal ","field":"SOURCE"}]}""");

        result.EventType.Should().Be(EventTypeCodes.CustomerAccountRegistered);
        result.Condition.Should().Be(
            """{"all":[{"field":"source","operator":"EQUALS","value":"NORMAL"}]}""");
    }

    [Fact]
    public void ParseCondition_EmptyAll_MatchesEveryValidSource()
    {
        var result = _service.ParseCondition(
            EventTypeCodes.CustomerAccountRegistered,
            """{"all":[]}""");

        result.Condition.Should().Be("""{"all":[]}""");
    }

    [Theory]
    [InlineData("""{"sources":["NORMAL"]}""")]
    [InlineData("""{"all":[{"field":"source","operator":"EQUALS","value":"NORMAL"}],"unexpected":true}""")]
    [InlineData("""{"all":[{"field":"source","operator":"EQUALS","value":"NORMAL"},{"field":"SOURCE","operator":"equals","value":"normal"}]}""")]
    public void ParseCondition_UnsupportedOrMalformedContract_Throws(string json)
    {
        var action = () => _service.ParseCondition(
            EventTypeCodes.CustomerAccountRegistered,
            json);

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("CAMPAIGN_CONDITION_INVALID");
    }

    [Fact]
    public void ParseAction_IssuePointForEventCustomer_ReturnsCanonicalEnvelope()
    {
        var result = _service.ParseAction(
            EventTypeCodes.CustomerAccountRegistered,
            " issue_point ",
            """
            {
              "parameters": { "amount": 50 },
              "target": { "selector": " event_customer " }
            }
            """);

        result.ActionType.Should().Be("ISSUE_POINT");
        result.ActionConfig.Should().Be(
            """{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50}}""");
    }

    [Fact]
    public void ParseAction_IssuePointForReferrer_ReturnsCanonicalEnvelope()
    {
        var result = _service.ParseAction(
            EventTypeCodes.CustomerAccountRegistered,
            "ISSUE_POINT",
            """{"target":{"selector":"referrer"},"parameters":{"amount":100}}""");

        using var document = JsonDocument.Parse(result.ActionConfig);
        document.RootElement.GetProperty("target").GetProperty("selector").GetString()
            .Should().Be("REFERRER");
        document.RootElement.GetProperty("parameters").GetProperty("amount").GetDecimal()
            .Should().Be(100m);
    }

    [Fact]
    public void GetActionUniquenessKey_NormalizesCanonicalActionConfig()
    {
        var normalized = _service.GetActionUniquenessKey(
            EventTypeCodes.CustomerAccountRegistered,
            " issue_point ",
            """{"parameters":{"amount":50},"target":{"selector":" event_customer "}}""");
        var canonical = _service.GetActionUniquenessKey(
            EventTypeCodes.CustomerAccountRegistered,
            "ISSUE_POINT",
            """{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50}}""");

        normalized.Should().Be(canonical);
    }

    [Fact]
    public void ReferrerAction_OnNormalCondition_IsRejected()
    {
        var action = () => _service.EnsureActionCompatibleWithCondition(
            EventTypeCodes.CustomerAccountRegistered,
            """{"all":[{"field":"source","operator":"EQUALS","value":"NORMAL"}]}""",
            "ISSUE_POINT",
            """{"target":{"selector":"REFERRER"},"parameters":{"amount":100}}""");

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_CONDITION_INCOMPATIBLE");
    }

    [Fact]
    public void ReferrerAction_OnAllCondition_IsAccepted()
    {
        var action = () => _service.EnsureActionCompatibleWithCondition(
            EventTypeCodes.CustomerAccountRegistered,
            """{"all":[]}""",
            "ISSUE_POINT",
            """{"target":{"selector":"REFERRER"},"parameters":{"amount":100}}""");

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("""
        {"calculationType":"PERCENT","recipient":"EVENT_CUSTOMER","amount":null,
         "calculationBase":"ORDER_AMOUNT","percentage":10,"maximumPoints":100}
        """)]
    [InlineData("""{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50.001}}""")]
    [InlineData("""{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50},"unexpected":true}""")]
    [InlineData("""{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50,"percentage":10}}""")]
    public void ParseAction_UnsupportedOrMalformedContract_Throws(string json)
    {
        var action = () => _service.ParseAction(
            EventTypeCodes.CustomerAccountRegistered,
            "ISSUE_POINT",
            json);

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_CONFIG_INVALID");
    }
}
