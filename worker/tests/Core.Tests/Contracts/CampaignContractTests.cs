using System.Text.Json;
using Campaign.Contracts.Campaigns.Actions;
using Campaign.Contracts.Constants;
using FluentAssertions;

namespace Core.Tests.Contracts;

public sealed class CampaignContractTests
{
    [Fact]
    public void CampaignAndSessionStatuses_ShouldRemainStable()
    {
        CampaignStatuses.Draft.Should().Be("DRAFT");
        CampaignStatuses.Active.Should().Be("ACTIVE");
        CampaignStatuses.Ended.Should().Be("ENDED");
        CampaignStatuses.Cancelled.Should().Be("CANCELLED");

        CampaignSessionStatuses.Scheduled.Should().Be("SCHEDULED");
        CampaignSessionStatuses.Running.Should().Be("RUNNING");
        CampaignSessionStatuses.Ended.Should().Be("ENDED");
        CampaignSessionStatuses.Cancelled.Should().Be("CANCELLED");
    }

    [Fact]
    public void CampaignCapabilities_ShouldIncludeCurrentAndReservedValues()
    {
        ActionReferenceTypes.Campaign.Should().Be("CAMPAIGN");
        ActionTypes.IssuePoint.Should().Be("ISSUE_POINT");
        PointCalculationTypes.FixedAmount.Should().Be("FIXED_AMOUNT");
        PointCalculationTypes.Percent.Should().Be("PERCENT");
        PointRecipients.EventCustomer.Should().Be("EVENT_CUSTOMER");
        PointRecipients.Referrer.Should().Be("REFERRER");
        CampaignScheduleDefaults.TimeZone.Should().Be("UTC");
        CampaignScheduleLimits.MaximumGeneratedSessions.Should().Be(10_000);
        CampaignScheduleDays.CanonicalOrder.Should().Equal(
            "MON",
            "TUE",
            "WED",
            "THU",
            "FRI",
            "SAT",
            "SUN");
    }

    [Fact]
    public void FixedAmountConfig_ShouldRoundTripThroughJson()
    {
        var config = new IssuePointActionConfig(
            PointCalculationTypes.FixedAmount,
            PointRecipients.EventCustomer,
            50m,
            null,
            null,
            null);

        var json = JsonSerializer.Serialize(config, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<IssuePointActionConfig>(json, JsonOptions);

        deserialized.Should().Be(config);
        json.Should().Contain("\"calculationType\":\"FIXED_AMOUNT\"");
        json.Should().Contain("\"recipient\":\"EVENT_CUSTOMER\"");
        json.Should().Contain("\"amount\":50");
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
