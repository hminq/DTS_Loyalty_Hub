using Campaign.Contracts.Constants;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Services;
using FluentAssertions;

namespace Core.Tests.CampaignProcessing;

public sealed class IssuePointActionConfigParserTests
{
    private readonly IssuePointActionConfigParser _parser = new();

    [Theory]
    [InlineData("EVENT_CUSTOMER")]
    [InlineData("REFERRER")]
    public void Parse_ValidFixedAmount_ReturnsTypedConfig(string recipient)
    {
        var result = _parser.Parse(
            ActionTypes.IssuePoint,
            $$"""
              {
                "calculationType": "FIXED_AMOUNT",
                "recipient": "{{recipient}}",
                "amount": 50.25,
                "calculationBase": null,
                "percentage": null,
                "maximumPoints": null
              }
              """);

        result.CalculationType.Should().Be(PointCalculationTypes.FixedAmount);
        result.Recipient.Should().Be(recipient);
        result.Amount.Should().Be(50.25m);
        result.CalculationBase.Should().BeNull();
        result.Percentage.Should().BeNull();
        result.MaximumPoints.Should().BeNull();
    }

    [Theory]
    [InlineData("OTHER", """
        {
          "calculationType": "FIXED_AMOUNT",
          "recipient": "EVENT_CUSTOMER",
          "amount": 50
        }
        """)]
    [InlineData("ISSUE_POINT", """
        {
          "calculationType": "PERCENT",
          "recipient": "EVENT_CUSTOMER",
          "percentage": 10
        }
        """)]
    [InlineData("ISSUE_POINT", """
        {
          "calculationType": "FIXED_AMOUNT",
          "recipient": "OTHER",
          "amount": 50
        }
        """)]
    [InlineData("ISSUE_POINT", """
        {
          "calculationType": "FIXED_AMOUNT",
          "recipient": "EVENT_CUSTOMER",
          "amount": 50,
          "unexpected": true
        }
        """)]
    [InlineData("ISSUE_POINT", "not-json")]
    [InlineData("ISSUE_POINT", "")]
    [InlineData("ISSUE_POINT", "   ")]
    public void Parse_InvalidConfiguration_ThrowsActionConfigurationInvalid(
        string actionType,
        string actionConfigJson)
    {
        var act = () => _parser.Parse(actionType, actionConfigJson);

        act.Should()
            .Throw<CampaignConfigurationException>()
            .Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.001")]
    [InlineData("10000000000000000")]
    public void Parse_InvalidAmount_ThrowsPointRewardAmountInvalid(string amount)
    {
        var config = $$"""
          {
            "calculationType": "FIXED_AMOUNT",
            "recipient": "EVENT_CUSTOMER",
            "amount": {{amount}},
            "calculationBase": null,
            "percentage": null,
            "maximumPoints": null
          }
          """;

        var act = () => _parser.Parse(ActionTypes.IssuePoint, config);

        act.Should()
            .Throw<CampaignConfigurationException>()
            .Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.PointRewardAmountInvalid);
    }
}
