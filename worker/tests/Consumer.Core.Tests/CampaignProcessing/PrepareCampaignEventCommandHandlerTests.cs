using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Handlers;
using Consumer.Core.Requests;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;

namespace Consumer.Core.Tests.CampaignProcessing;

public sealed class PrepareCampaignEventCommandHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly ValidatedCustomerAccountRegisteredEvent CampaignEvent = new(
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        EventTypeCodes.CustomerAccountRegistered,
        EventRoutingKeys.CustomerAccountRegistered,
        new DateTime(2026, 7, 28, 10, 0, 0, DateTimeKind.Utc),
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
        CustomerRegistrationSources.Normal,
        null,
        """{"eventId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"}""",
        new string('a', 64));

    private readonly Mock<ICampaignEventPreparationStore> _store = new();

    [Fact]
    public void Command_ShouldOwnExplicitTransactionBoundary()
    {
        var command = new PrepareCampaignEventCommand(CampaignEvent);

        command.Should().BeAssignableTo<ITransactionalRequest>();
    }

    [Fact]
    public async Task Handle_FirstDeliveryWithCandidates_PinsStableOrderedTargets()
    {
        var campaignId1 = Guid.Parse("10000000-0000-0000-0000-000000000000");
        var campaignId2 = Guid.Parse("20000000-0000-0000-0000-000000000000");
        var sessionId1 = Guid.Parse("11000000-0000-0000-0000-000000000000");
        var sessionId2 = Guid.Parse("22000000-0000-0000-0000-000000000000");
        SetupInsertedParent();
        _store.Setup(store => store.GetCandidateTargetsAsync(
                CampaignEvent.EventType,
                CampaignEvent.OccurredAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CampaignEventCandidate(campaignId2, sessionId2),
                new CampaignEventCandidate(campaignId1, sessionId1)
            ]);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new PrepareCampaignEventCommand(CampaignEvent),
            CancellationToken.None);

        result.Created.Should().BeTrue();
        result.Status.Should().Be(EventProcessingStatuses.Pending);
        result.Targets.Select(target => target.CampaignId)
            .Should().Equal(campaignId1, campaignId2);
        result.Targets.Should().OnlyContain(target =>
            target.EventCampaignProcessingId != Guid.Empty &&
            target.Status == EventCampaignProcessingStatuses.Pending);
        result.Targets.Select(target => target.EventCampaignProcessingId)
            .Should().OnlyHaveUniqueItems();
        _store.Verify(store => store.AddTargets(
            CampaignEvent.EventId,
            CampaignEvent.CustomerId,
            It.Is<IReadOnlyList<PreparedCampaignTarget>>(targets => targets.Count == 2),
            Now.UtcDateTime), Times.Once);
        _store.Verify(store => store.MarkEventCompletedAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FirstDeliveryWithoutCandidates_CompletesParent()
    {
        SetupInsertedParent();
        _store.Setup(store => store.GetCandidateTargetsAsync(
                CampaignEvent.EventType,
                CampaignEvent.OccurredAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new PrepareCampaignEventCommand(CampaignEvent),
            CancellationToken.None);

        result.Should().Be(new PrepareCampaignEventResult(
            CampaignEvent.EventId,
            EventProcessingStatuses.Completed,
            Created: true,
            Array.Empty<PreparedCampaignTarget>()));
        _store.Verify(store => store.MarkEventCompletedAsync(
            CampaignEvent.EventId,
            Now.UtcDateTime,
            It.IsAny<CancellationToken>()), Times.Once);
        _store.Verify(store => store.AddTargets(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<IReadOnlyList<PreparedCampaignTarget>>(),
            It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Redelivery_ReusesPersistedTargetsWithoutCandidateQuery()
    {
        var persistedTarget = new PreparedCampaignTarget(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            EventCampaignProcessingStatuses.Completed);
        _store.Setup(store => store.TryInsertEventAsync(
                CampaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _store.Setup(store => store.GetEventStateAsync(
                CampaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateState(
                EventProcessingStatuses.Pending,
                [persistedTarget]));
        var handler = CreateHandler();

        var result = await handler.Handle(
            new PrepareCampaignEventCommand(CampaignEvent),
            CancellationToken.None);

        result.Created.Should().BeFalse();
        result.Targets.Should().Equal(persistedTarget);
        _store.Verify(store => store.GetCandidateTargetsAsync(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _store.Verify(store => store.AddTargets(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<IReadOnlyList<PreparedCampaignTarget>>(),
            It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateEventIdWithDifferentHash_ThrowsCollision()
    {
        _store.Setup(store => store.TryInsertEventAsync(
                CampaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _store.Setup(store => store.GetEventStateAsync(
                CampaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateState(
                EventProcessingStatuses.Pending,
                [],
                payloadHash: new string('b', 64)));
        var handler = CreateHandler();

        var act = () => handler.Handle(
            new PrepareCampaignEventCommand(CampaignEvent),
            CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<CampaignEventValidationException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.EventIdCollision);
    }

    [Fact]
    public async Task Handle_OverlappingCandidateSessions_ThrowsInvalidConfiguration()
    {
        var campaignId = Guid.NewGuid();
        SetupInsertedParent();
        _store.Setup(store => store.GetCandidateTargetsAsync(
                CampaignEvent.EventType,
                CampaignEvent.OccurredAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CampaignEventCandidate(campaignId, Guid.NewGuid()),
                new CampaignEventCandidate(campaignId, Guid.NewGuid())
            ]);
        var handler = CreateHandler();

        var act = () => handler.Handle(
            new PrepareCampaignEventCommand(CampaignEvent),
            CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<CampaignConfigurationException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignConfigurationInvalid);
        _store.Verify(store => store.AddTargets(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<IReadOnlyList<PreparedCampaignTarget>>(),
            It.IsAny<DateTime>()), Times.Never);
    }

    private PrepareCampaignEventCommandHandler CreateHandler()
    {
        return new PrepareCampaignEventCommandHandler(
            _store.Object,
            new FixedTimeProvider(Now));
    }

    private void SetupInsertedParent()
    {
        _store.Setup(store => store.TryInsertEventAsync(
                CampaignEvent,
                Now.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _store.Setup(store => store.GetEventStateAsync(
                CampaignEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateState(EventProcessingStatuses.Pending, []));
    }

    private static EventPreparationState CreateState(
        string status,
        IReadOnlyList<PreparedCampaignTarget> targets,
        string? payloadHash = null)
    {
        return new EventPreparationState(
            CampaignEvent.EventId,
            CampaignEvent.EventType,
            CampaignEvent.RoutingKey,
            CampaignEvent.OccurredAt,
            payloadHash ?? CampaignEvent.PayloadHash,
            status,
            targets);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
