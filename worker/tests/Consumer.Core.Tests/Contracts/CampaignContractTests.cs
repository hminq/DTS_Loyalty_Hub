using Campaign.Contracts.Constants;
using FluentAssertions;

namespace Consumer.Core.Tests.Contracts;

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
        CustomerRegistrationTargetSelectors.EventCustomer.Should()
            .Be("EVENT_CUSTOMER");
        CustomerRegistrationTargetSelectors.Referrer.Should()
            .Be("REFERRER");
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

}
