using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Handlers;
using Consumer.Core.Requests;
using FluentAssertions;
using Moq;

namespace Consumer.Core.Tests.CampaignProcessing;

public sealed class PrepareCampaignEventCommandHandlerPinningTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 30, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<ICampaignEventPreparationStore> _store = new();

    [Fact]
    public async Task Handle_FirstSeenEvent_PinsExactVersionCandidatesInDeterministicOrder()
    {
        var campaignEvent = CreateEvent();
        var campaignId1 = Guid.Parse("10000000-0000-0000-0000-000000000000");
        var campaignId2 = Guid.Parse("20000000-0000-0000-0000-000000000000");
        var sessionId1 = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var sessionId2 = Guid.Parse("20000000-0000-0000-0000-000000000001");

        _store.Setup(x => x.TryInsertEventAsync(
                campaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _store.Setup(x => x.GetEventStateAsync(
                campaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateState(campaignEvent, EventProcessingStatuses.Pending, []));
        _store.Setup(x => x.GetCandidateTargetsAsync(
                campaignEvent.EventTypeVersionId,
                campaignEvent.OccurredAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CampaignEventCandidate(campaignId2, sessionId2),
                new CampaignEventCandidate(campaignId1, sessionId1)
            ]);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new PrepareCampaignEventCommand(campaignEvent),
            CancellationToken.None);

        result.Created.Should().BeTrue();
        result.Status.Should().Be(EventProcessingStatuses.Pending);
        result.Targets.Select(target => (target.CampaignId, target.CampaignSessionId))
            .Should().Equal(
                (campaignId1, sessionId1),
                (campaignId2, sessionId2));

        _store.Verify(x => x.GetCandidateTargetsAsync(
            campaignEvent.EventTypeVersionId,
            campaignEvent.OccurredAt,
            It.IsAny<CancellationToken>()), Times.Once);
        _store.Verify(x => x.AddTargets(
            campaignEvent.EventId,
            It.Is<IReadOnlyList<PreparedCampaignTarget>>(targets =>
                targets.Count == 2 &&
                targets.All(target => target.Status == EventCampaignProcessingStatuses.Pending)),
            Now.UtcDateTime), Times.Once);
    }

    [Fact]
    public async Task Handle_FirstSeenEventWithNoCandidates_CompletesEventWithoutChildren()
    {
        var campaignEvent = CreateEvent();

        _store.Setup(x => x.TryInsertEventAsync(
                campaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _store.Setup(x => x.GetEventStateAsync(
                campaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateState(campaignEvent, EventProcessingStatuses.Pending, []));
        _store.Setup(x => x.GetCandidateTargetsAsync(
                campaignEvent.EventTypeVersionId,
                campaignEvent.OccurredAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new PrepareCampaignEventCommand(campaignEvent),
            CancellationToken.None);

        result.Created.Should().BeTrue();
        result.Status.Should().Be(EventProcessingStatuses.Completed);
        result.Targets.Should().BeEmpty();

        _store.Verify(x => x.MarkEventCompletedAsync(
            campaignEvent.EventId,
            Now.UtcDateTime,
            It.IsAny<CancellationToken>()), Times.Once);
        _store.Verify(x => x.AddTargets(
            It.IsAny<Guid>(),
            It.IsAny<IReadOnlyList<PreparedCampaignTarget>>(),
            It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingEvent_ReturnsPersistedChildrenWithoutCandidateLookup()
    {
        var campaignEvent = CreateEvent();
        var persistedTargets = new[]
        {
            new PreparedCampaignTarget(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                EventCampaignProcessingStatuses.Completed),
            new PreparedCampaignTarget(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                EventCampaignProcessingStatuses.Pending)
        };

        _store.Setup(x => x.TryInsertEventAsync(
                campaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _store.Setup(x => x.GetEventStateAsync(
                campaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateState(campaignEvent, EventProcessingStatuses.Pending, persistedTargets));

        var handler = CreateHandler();

        var result = await handler.Handle(
            new PrepareCampaignEventCommand(campaignEvent),
            CancellationToken.None);

        result.Created.Should().BeFalse();
        result.Targets.Should().Equal(persistedTargets);

        _store.Verify(x => x.GetCandidateTargetsAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _store.Verify(x => x.AddTargets(
            It.IsAny<Guid>(),
            It.IsAny<IReadOnlyList<PreparedCampaignTarget>>(),
            It.IsAny<DateTime>()), Times.Never);
        _store.Verify(x => x.MarkEventCompletedAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("eventType")]
    [InlineData("eventTypeVersionId")]
    [InlineData("eventVersion")]
    [InlineData("routingKey")]
    [InlineData("occurredAt")]
    [InlineData("payloadHash")]
    public async Task Handle_ExistingEventWithIdentityMismatch_ThrowsEventIdCollision(
        string mismatch)
    {
        var campaignEvent = CreateEvent();
        var state = mismatch switch
        {
            "eventType" => CreateState(campaignEvent with { EventType = "OTHER_EVENT" }),
            "eventTypeVersionId" => CreateState(campaignEvent with { EventTypeVersionId = Guid.NewGuid() }),
            "eventVersion" => CreateState(campaignEvent with { EventVersion = 2 }),
            "routingKey" => CreateState(campaignEvent with { RoutingKey = "other.routing" }),
            "occurredAt" => CreateState(campaignEvent with { OccurredAt = campaignEvent.OccurredAt.AddTicks(10) }),
            "payloadHash" => CreateState(campaignEvent with { PayloadHash = "different-hash" }),
            _ => throw new InvalidOperationException("Unknown mismatch.")
        };

        _store.Setup(x => x.TryInsertEventAsync(
                campaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _store.Setup(x => x.GetEventStateAsync(
                campaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var handler = CreateHandler();

        var act = () => handler.Handle(
            new PrepareCampaignEventCommand(campaignEvent),
            CancellationToken.None);

        await act.Should().ThrowAsync<CampaignEventValidationException>()
            .WithMessage(CampaignProcessingErrorCodes.EventIdCollision);

        _store.Verify(x => x.GetCandidateTargetsAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingPersistedStateAfterInsertAttempt_ThrowsRetriablePersistenceFailure()
    {
        var campaignEvent = CreateEvent();

        _store.Setup(x => x.TryInsertEventAsync(
                campaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _store.Setup(x => x.GetEventStateAsync(
                campaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventPreparationState?)null);

        var handler = CreateHandler();

        var act = () => handler.Handle(
            new PrepareCampaignEventCommand(campaignEvent),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<CampaignProcessingException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed);
        exception.Which.Retriable.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DuplicateEligibleSessionsForOneCampaign_ThrowsInvalidConfiguration()
    {
        var campaignEvent = CreateEvent();
        var campaignId = Guid.NewGuid();

        _store.Setup(x => x.TryInsertEventAsync(
                campaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _store.Setup(x => x.GetEventStateAsync(
                campaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateState(campaignEvent));
        _store.Setup(x => x.GetCandidateTargetsAsync(
                campaignEvent.EventTypeVersionId,
                campaignEvent.OccurredAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CampaignEventCandidate(campaignId, Guid.NewGuid()),
                new CampaignEventCandidate(campaignId, Guid.NewGuid())
            ]);

        var handler = CreateHandler();

        var act = () => handler.Handle(
            new PrepareCampaignEventCommand(campaignEvent),
            CancellationToken.None);

        await act.Should().ThrowAsync<CampaignConfigurationException>()
            .WithMessage(CampaignProcessingErrorCodes.CampaignConfigurationInvalid);
    }

    private PrepareCampaignEventCommandHandler CreateHandler()
    {
        return new PrepareCampaignEventCommandHandler(
            _store.Object,
            new FixedTimeProvider(Now));
    }

    private static TestValidatedCampaignEvent CreateEvent()
    {
        return new TestValidatedCampaignEvent(
            Guid.NewGuid(),
            "TEST_EVENT",
            Guid.NewGuid(),
            1,
            "test.routing",
            new DateTime(2026, 7, 30, 8, 0, 0, DateTimeKind.Utc),
            "{\"eventId\":\"x\"}",
            "payload-hash");
    }

    private static EventPreparationState CreateState(
        IValidatedCampaignEvent campaignEvent,
        string status = EventProcessingStatuses.Pending,
        IReadOnlyList<PreparedCampaignTarget>? targets = null)
    {
        return new EventPreparationState(
            campaignEvent.EventId,
            campaignEvent.EventType,
            campaignEvent.EventTypeVersionId,
            campaignEvent.EventVersion,
            campaignEvent.RoutingKey,
            campaignEvent.OccurredAt,
            campaignEvent.PayloadHash,
            status,
            targets ?? []);
    }

    private sealed record TestValidatedCampaignEvent(
        Guid EventId,
        string EventType,
        Guid EventTypeVersionId,
        int EventVersion,
        string RoutingKey,
        DateTime OccurredAt,
        string NormalizedPayload,
        string PayloadHash)
        : IValidatedCampaignEvent;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
