using System.Text.Json;
using Core.Exceptions;
using Core.UseCases.Campaigns;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Core.Tests.UseCases.Campaigns;

public sealed class CampaignConfigurationParserTests
{
    [Fact]
    public void ParseCondition_NormalSource_ReturnsCanonicalContract()
    {
        const string json = """{"sources":[" normal "]}""";

        var result = CampaignConfigurationParser.ParseCondition(
            " customer_account_registered ",
            json);

        result.EventType.Should().Be(EventTypeCodes.CustomerAccountRegistered);
        using var document = JsonDocument.Parse(result.Condition);
        document.RootElement.GetProperty("sources")[0].GetString().Should().Be("NORMAL");
    }

    [Theory]
    [InlineData("""{"sources":["REFERRAL"]}""")]
    [InlineData("""{"sources":["NORMAL","NORMAL"]}""")]
    [InlineData("""{"sources":["NORMAL"],"unexpected":true}""")]
    public void ParseCondition_UnsupportedOrMalformedContract_Throws(string json)
    {
        var action = () => CampaignConfigurationParser.ParseCondition(
            EventTypeCodes.CustomerAccountRegistered,
            json);

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("CAMPAIGN_CONDITION_INVALID");
    }

    [Fact]
    public void ParseCondition_EmptyCondition_MatchesEveryValidSource()
    {
        var result = CampaignConfigurationParser.ParseCondition(
            EventTypeCodes.CustomerAccountRegistered,
            "{}");

        using var document = JsonDocument.Parse(result.Condition);
        document.RootElement.GetProperty("sources").ValueKind
            .Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void ParseAction_FixedAmountForEventCustomer_ReturnsCanonicalContract()
    {
        const string json = """
            {
              "calculationType": " fixed_amount ",
              "recipient": " event_customer ",
              "amount": 50,
              "calculationBase": null,
              "percentage": null,
              "maximumPoints": null
            }
            """;

        var result = CampaignConfigurationParser.ParseAction(
            EventTypeCodes.CustomerAccountRegistered,
            " issue_point ",
            json);

        result.ActionType.Should().Be("ISSUE_POINT");
        using var document = JsonDocument.Parse(result.ActionConfig);
        document.RootElement.GetProperty("calculationType").GetString()
            .Should().Be("FIXED_AMOUNT");
        document.RootElement.GetProperty("recipient").GetString()
            .Should().Be("EVENT_CUSTOMER");
        document.RootElement.GetProperty("amount").GetDecimal().Should().Be(50);
    }

    [Theory]
    [InlineData("""
        {"calculationType":"PERCENT","recipient":"EVENT_CUSTOMER","amount":null,
         "calculationBase":"ORDER_AMOUNT","percentage":10,"maximumPoints":100}
        """)]
    [InlineData("""
        {"calculationType":"FIXED_AMOUNT","recipient":"REFERRER","amount":50,
         "calculationBase":null,"percentage":null,"maximumPoints":null}
        """)]
    [InlineData("""
        {"calculationType":"FIXED_AMOUNT","recipient":"EVENT_CUSTOMER","amount":50.001,
         "calculationBase":null,"percentage":null,"maximumPoints":null}
        """)]
    [InlineData("""
        {"calculationType":"FIXED_AMOUNT","recipient":"EVENT_CUSTOMER","amount":50,
         "calculationBase":null,"percentage":null,"maximumPoints":null,"unexpected":true}
        """)]
    public void ParseAction_UnsupportedOrMalformedContract_Throws(string json)
    {
        var action = () => CampaignConfigurationParser.ParseAction(
            EventTypeCodes.CustomerAccountRegistered,
            "ISSUE_POINT",
            json);

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_CONFIG_INVALID");
    }
}
