using Campaign.Contracts.Constants;
using Scheduler.Core.Abstractions;
using Scheduler.Core.Entities;
using Scheduler.Core.Handlers;
using Scheduler.Core.Requests;
using FluentAssertions;
using Moq;
using Xunit;

namespace Scheduler.Core.Tests.Handlers;

public sealed class ProcessCampaignSessionLifecycleBatchCommandHandlerTests
{
    private const int BatchSize = 100;
    private static readonly DateTime ProcessedAt = new(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ICampaignSessionLifecycleStore> _store = new();

    [Fact]
    public async Task Handle_NonUtcProcessedAt_ThrowsArgumentException()
    {
        var command = new ProcessCampaignSessionLifecycleBatchCommand(DateTime.Now, BatchSize);
        var handler = new ProcessCampaignSessionLifecycleBatchCommandHandler(_store.Object);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_InvalidBatchSize_ThrowsArgumentException()
    {
        var command = new ProcessCampaignSessionLifecycleBatchCommand(ProcessedAt, 0);
        var handler = new ProcessCampaignSessionLifecycleBatchCommandHandler(_store.Object);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_DueScheduledSession_TransitionsToRunning()
    {
        var sessionId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var scheduledCandidate = new CampaignSessionLifecycleCandidate(
            sessionId,
            campaignId,
            ProcessedAt.AddHours(-1),
            ProcessedAt.AddHours(1),
            CampaignSessionStatuses.Scheduled,
            null);

        _store.Setup(s => s.GetDueScheduledSessionsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync([scheduledCandidate]);
        _store.Setup(s => s.GetDueRunningSessionsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CampaignSessionLifecycleCandidate>());

        var handler = new ProcessCampaignSessionLifecycleBatchCommandHandler(_store.Object);
        var result = await handler.Handle(
            new ProcessCampaignSessionLifecycleBatchCommand(ProcessedAt, BatchSize),
            CancellationToken.None);

        result.Should().Be(new ProcessCampaignSessionLifecycleBatchResult(1, 0, 1, 0));
        _store.Verify(s => s.ApplySessionMutationsAsync(
            It.Is<IReadOnlyList<CampaignSessionLifecycleMutation>>(mutations =>
                mutations.Count == 1 &&
                mutations[0].CampaignSessionId == sessionId &&
                mutations[0].TargetStatus == CampaignSessionStatuses.Running &&
                mutations[0].EndedAt == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissedScheduledSession_TransitionsToEnded()
    {
        var sessionId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var missedCandidate = new CampaignSessionLifecycleCandidate(
            sessionId,
            campaignId,
            ProcessedAt.AddHours(-3),
            ProcessedAt.AddHours(-1),
            CampaignSessionStatuses.Scheduled,
            null);

        _store.Setup(s => s.GetDueScheduledSessionsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync([missedCandidate]);
        _store.Setup(s => s.GetDueRunningSessionsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CampaignSessionLifecycleCandidate>());

        var handler = new ProcessCampaignSessionLifecycleBatchCommandHandler(_store.Object);
        var result = await handler.Handle(
            new ProcessCampaignSessionLifecycleBatchCommand(ProcessedAt, BatchSize),
            CancellationToken.None);

        result.Should().Be(new ProcessCampaignSessionLifecycleBatchResult(1, 0, 0, 1));
        _store.Verify(s => s.ApplySessionMutationsAsync(
            It.Is<IReadOnlyList<CampaignSessionLifecycleMutation>>(mutations =>
                mutations.Count == 1 &&
                mutations[0].CampaignSessionId == sessionId &&
                mutations[0].TargetStatus == CampaignSessionStatuses.Ended &&
                mutations[0].EndedAt == ProcessedAt),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DueRunningSession_TransitionsToEnded()
    {
        var sessionId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var runningCandidate = new CampaignSessionLifecycleCandidate(
            sessionId,
            campaignId,
            ProcessedAt.AddHours(-2),
            ProcessedAt,
            CampaignSessionStatuses.Running,
            null);

        _store.Setup(s => s.GetDueScheduledSessionsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CampaignSessionLifecycleCandidate>());
        _store.Setup(s => s.GetDueRunningSessionsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync([runningCandidate]);

        var handler = new ProcessCampaignSessionLifecycleBatchCommandHandler(_store.Object);
        var result = await handler.Handle(
            new ProcessCampaignSessionLifecycleBatchCommand(ProcessedAt, BatchSize),
            CancellationToken.None);

        result.Should().Be(new ProcessCampaignSessionLifecycleBatchResult(0, 1, 0, 1));
        _store.Verify(s => s.ApplySessionMutationsAsync(
            It.Is<IReadOnlyList<CampaignSessionLifecycleMutation>>(mutations =>
                mutations.Count == 1 &&
                mutations[0].CampaignSessionId == sessionId &&
                mutations[0].TargetStatus == CampaignSessionStatuses.Ended &&
                mutations[0].EndedAt == ProcessedAt),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoCandidates_DoesNotCallApplySessionMutations()
    {
        _store.Setup(s => s.GetDueScheduledSessionsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CampaignSessionLifecycleCandidate>());
        _store.Setup(s => s.GetDueRunningSessionsForUpdateAsync(ProcessedAt, BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CampaignSessionLifecycleCandidate>());

        var handler = new ProcessCampaignSessionLifecycleBatchCommandHandler(_store.Object);
        var result = await handler.Handle(
            new ProcessCampaignSessionLifecycleBatchCommand(ProcessedAt, BatchSize),
            CancellationToken.None);

        result.Should().Be(new ProcessCampaignSessionLifecycleBatchResult(0, 0, 0, 0));
        _store.Verify(s => s.ApplySessionMutationsAsync(It.IsAny<IReadOnlyList<CampaignSessionLifecycleMutation>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
