using Core.Entities.Constants;
using FluentAssertions;

namespace Core.Tests.CampaignProcessing;

public sealed class CampaignProcessingConstantsTests
{
    [Fact]
    public void ProcessingConstants_ShouldMatchDatabaseContracts()
    {
        EventProcessingStatuses.Pending.Should().Be("PENDING");
        EventProcessingStatuses.Completed.Should().Be("COMPLETED");
        EventProcessingStatuses.Failed.Should().Be("FAILED");

        EventCampaignProcessingStatuses.Pending.Should().Be("PENDING");
        EventCampaignProcessingStatuses.Completed.Should().Be("COMPLETED");
        EventCampaignProcessingStatuses.Skipped.Should().Be("SKIPPED");
        EventCampaignProcessingStatuses.Failed.Should().Be("FAILED");

        PointTransactionTypes.CampaignReward.Should().Be("CAMPAIGN_REWARD");
    }
}
