using Scheduler.Core.Abstractions;
using Scheduler.Core.Handlers;
using Scheduler.Core.Requests;
using FluentAssertions;
using Moq;
using Xunit;

namespace Scheduler.Core.Tests.Handlers;

public sealed class EndEligibleCampaignsBatchCommandHandlerTests
{
    private const int BatchSize = 100;
    private static readonly DateTime ProcessedAt = new(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ICampaignSessionLifecycleStore> _store = new();

    [Fact]
    public async Task Handle_NonUtcProcessedAt_ThrowsArgumentException()
    {
        var command = new EndEligibleCampaignsBatchCommand(DateTime.Now, BatchSize);
        var handler = new EndEligibleCampaignsBatchCommandHandler(_store.Object);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_EligibleCampaignsExist_EndsCampaigns()
    {
        var campaignId1 = Guid.NewGuid();
        var campaignId2 = Guid.NewGuid();
        _store.Setup(s => s.GetEligibleCampaignsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync([campaignId1, campaignId2]);

        var handler = new EndEligibleCampaignsBatchCommandHandler(_store.Object);
        var result = await handler.Handle(
            new EndEligibleCampaignsBatchCommand(ProcessedAt, BatchSize),
            CancellationToken.None);

        result.Should().Be(new EndEligibleCampaignsBatchResult(2, 2));
        _store.Verify(s => s.EndCampaignsAsync(
            It.Is<IReadOnlyList<Guid>>(ids =>
                ids.Count == 2 &&
                ids.Contains(campaignId1) &&
                ids.Contains(campaignId2)),
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoEligibleCampaigns_DoesNotCallEndCampaigns()
    {
        _store.Setup(s => s.GetEligibleCampaignsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());

        var handler = new EndEligibleCampaignsBatchCommandHandler(_store.Object);
        var result = await handler.Handle(
            new EndEligibleCampaignsBatchCommand(ProcessedAt, BatchSize),
            CancellationToken.None);

        result.Should().Be(new EndEligibleCampaignsBatchResult(0, 0));
        _store.Verify(s => s.EndCampaignsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
