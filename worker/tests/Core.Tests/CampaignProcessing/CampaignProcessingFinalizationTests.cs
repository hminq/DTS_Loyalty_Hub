using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Handlers;
using Core.Requests;
using Core.Services;
using FluentAssertions;
using Moq;

namespace Core.Tests.CampaignProcessing;

public sealed class CampaignProcessingFinalizationTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 28, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RecordFailure_PendingChild_SanitizesAndMarksFailed()
    {
        var childId = Guid.NewGuid();
        var store = new Mock<IEventProcessingFinalizationStore>();
        store.Setup(item => item.LockCampaignProcessingAsync(
                childId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampaignProcessingFailureState(
                childId,
                EventCampaignProcessingStatuses.Pending,
                2,
                null));
        var handler = new RecordCampaignProcessingFailureCommandHandler(
            store.Object,
            new FixedTimeProvider(Now));

        var result = await handler.Handle(
            new RecordCampaignProcessingFailureCommand(
                childId,
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
                "  database\r\n details\t hidden  "),
            CancellationToken.None);

        result.Should().Be(new RecordCampaignProcessingFailureResult(
            childId,
            EventCampaignProcessingStatuses.Failed,
            CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
            3,
            Updated: true));
        store.Verify(item => item.MarkCampaignFailed(
            childId,
            CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
            "database details hidden",
            Now.UtcDateTime), Times.Once);
    }

    [Theory]
    [InlineData(EventCampaignProcessingStatuses.Completed)]
    [InlineData(EventCampaignProcessingStatuses.Skipped)]
    [InlineData(EventCampaignProcessingStatuses.Failed)]
    public async Task RecordFailure_TerminalChild_IsNoOp(string status)
    {
        var childId = Guid.NewGuid();
        var store = new Mock<IEventProcessingFinalizationStore>();
        store.Setup(item => item.LockCampaignProcessingAsync(
                childId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampaignProcessingFailureState(
                childId,
                status,
                1,
                "EXISTING_OUTCOME"));
        var handler = new RecordCampaignProcessingFailureCommandHandler(
            store.Object,
            new FixedTimeProvider(Now));

        var result = await handler.Handle(
            new RecordCampaignProcessingFailureCommand(
                childId,
                CampaignProcessingErrorCodes.CampaignProcessingUnexpectedError,
                "ignored"),
            CancellationToken.None);

        result.Updated.Should().BeFalse();
        result.Status.Should().Be(status);
        result.AttemptCount.Should().Be(1);
        store.Verify(item => item.MarkCampaignFailed(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Finalize_AllCompletedOrSkipped_CompletesParent()
    {
        var eventId = Guid.NewGuid();
        var store = SetupFinalizationStore(
            eventId,
            [
                EventCampaignProcessingStatuses.Completed,
                EventCampaignProcessingStatuses.Skipped
            ]);
        var handler = CreateFinalizer(store.Object);

        var result = await handler.Handle(
            new FinalizeEventProcessingCommand(eventId),
            CancellationToken.None);

        result.Status.Should().Be(EventProcessingStatuses.Completed);
        result.CompletedCampaignCount.Should().Be(1);
        result.SkippedCampaignCount.Should().Be(1);
        result.Updated.Should().BeTrue();
        store.Verify(item => item.MarkEventCompleted(
            eventId,
            Now.UtcDateTime), Times.Once);
    }

    [Fact]
    public async Task Finalize_ZeroChildren_CompletesParent()
    {
        var eventId = Guid.NewGuid();
        var store = SetupFinalizationStore(eventId, []);
        var handler = CreateFinalizer(store.Object);

        var result = await handler.Handle(
            new FinalizeEventProcessingCommand(eventId),
            CancellationToken.None);

        result.Status.Should().Be(EventProcessingStatuses.Completed);
        store.Verify(item => item.MarkEventCompleted(
            eventId,
            Now.UtcDateTime), Times.Once);
    }

    [Fact]
    public async Task Finalize_AnyFailedChild_FailsParent()
    {
        var eventId = Guid.NewGuid();
        var store = SetupFinalizationStore(
            eventId,
            [
                EventCampaignProcessingStatuses.Completed,
                EventCampaignProcessingStatuses.Failed,
                EventCampaignProcessingStatuses.Pending
            ]);
        var handler = CreateFinalizer(store.Object);

        var result = await handler.Handle(
            new FinalizeEventProcessingCommand(eventId),
            CancellationToken.None);

        result.Status.Should().Be(EventProcessingStatuses.Failed);
        result.FailedCampaignCount.Should().Be(1);
        result.PendingCampaignCount.Should().Be(1);
        store.Verify(item => item.MarkEventFailed(
            eventId,
            Now.UtcDateTime), Times.Once);
    }

    [Fact]
    public async Task Finalize_PendingWithoutFailure_LeavesParentPending()
    {
        var eventId = Guid.NewGuid();
        var store = SetupFinalizationStore(
            eventId,
            [EventCampaignProcessingStatuses.Pending]);
        var handler = CreateFinalizer(store.Object);

        var result = await handler.Handle(
            new FinalizeEventProcessingCommand(eventId),
            CancellationToken.None);

        result.Status.Should().Be(EventProcessingStatuses.Pending);
        result.Updated.Should().BeFalse();
        store.Verify(item => item.MarkEventCompleted(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>()), Times.Never);
        store.Verify(item => item.MarkEventFailed(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Coordinator_FailedCampaignIsRecordedAndLaterCampaignContinues()
    {
        var eventId = Guid.NewGuid();
        var completedTarget = Target(
            "00000000-0000-0000-0000-000000000001",
            EventCampaignProcessingStatuses.Completed);
        var failedTarget = Target(
            "00000000-0000-0000-0000-000000000002",
            EventCampaignProcessingStatuses.Pending);
        var laterTarget = Target(
            "00000000-0000-0000-0000-000000000003",
            EventCampaignProcessingStatuses.Pending);
        var executor = new Mock<ICampaignProcessingScopeExecutor>();
        executor.Setup(item => item.ProcessCampaignAsync(
                failedTarget.EventCampaignProcessingId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CampaignConfigurationException(
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid));
        executor.Setup(item => item.RecordFailureAsync(
                failedTarget.EventCampaignProcessingId,
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RecordCampaignProcessingFailureResult(
                failedTarget.EventCampaignProcessingId,
                EventCampaignProcessingStatuses.Failed,
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
                1,
                Updated: true));
        executor.Setup(item => item.ProcessCampaignAsync(
                laterTarget.EventCampaignProcessingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessCustomerRegistrationCampaignResult(
                laterTarget.EventCampaignProcessingId,
                EventCampaignProcessingStatuses.Completed,
                null,
                1,
                1));
        executor.Setup(item => item.FinalizeEventAsync(
                eventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FinalizeEventProcessingResult(
                eventId,
                EventProcessingStatuses.Failed,
                2,
                0,
                1,
                0,
                Updated: true));
        var coordinator = new CampaignEventProcessingCoordinator(executor.Object);

        var result = await coordinator.ProcessAsync(
            eventId,
            [laterTarget, completedTarget, failedTarget],
            CancellationToken.None);

        result.CampaignResults.Should().ContainSingle(
            item => item.EventCampaignProcessingId ==
                laterTarget.EventCampaignProcessingId);
        result.FailureResults.Should().ContainSingle(
            item => item.EventCampaignProcessingId ==
                failedTarget.EventCampaignProcessingId);
        result.Finalization.Status.Should().Be(EventProcessingStatuses.Failed);
        executor.Verify(item => item.ProcessCampaignAsync(
            completedTarget.EventCampaignProcessingId,
            It.IsAny<CancellationToken>()), Times.Never);
        executor.Verify(item => item.ProcessCampaignAsync(
            laterTarget.EventCampaignProcessingId,
            It.IsAny<CancellationToken>()), Times.Once);
        executor.Verify(item => item.FinalizeEventAsync(
            eventId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static FinalizeEventProcessingCommandHandler CreateFinalizer(
        IEventProcessingFinalizationStore store)
    {
        return new FinalizeEventProcessingCommandHandler(
            store,
            new FixedTimeProvider(Now));
    }

    private static Mock<IEventProcessingFinalizationStore> SetupFinalizationStore(
        Guid eventId,
        IReadOnlyList<string> childStatuses)
    {
        var store = new Mock<IEventProcessingFinalizationStore>();
        store.Setup(item => item.LockEventAsync(
                eventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventProcessingFinalizationState(
                eventId,
                EventProcessingStatuses.Pending,
                childStatuses));
        return store;
    }

    private static PreparedCampaignTarget Target(
        string campaignId,
        string status)
    {
        return new PreparedCampaignTarget(
            Guid.NewGuid(),
            Guid.Parse(campaignId),
            Guid.NewGuid(),
            status);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
