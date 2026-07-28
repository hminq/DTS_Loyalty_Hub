using System.Text;
using System.Text.Json;
using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Handlers;
using Core.Requests;
using Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;

namespace Core.Tests.CampaignProcessing;

public sealed class ProcessCustomerRegistrationCampaignCommandHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<ICampaignRewardExecutionStore> _store = new();

    [Fact]
    public async Task Handle_CompletedChild_ReturnsStoredOutcomeWithoutExecuting()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(
            campaignEvent,
            status: EventCampaignProcessingStatuses.Completed,
            attemptCount: 1);
        _store.Setup(store => store.LockProcessingAsync(
                context.EventCampaignProcessingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var result = await CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        result.Status.Should().Be(EventCampaignProcessingStatuses.Completed);
        result.AttemptCount.Should().Be(1);
        result.ExecutedActionCount.Should().Be(0);
        _store.Verify(store => store.LockProcessingAsync(
            context.EventCampaignProcessingId,
            It.IsAny<CancellationToken>()), Times.Once);
        _store.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ConditionMismatch_SkipsWithoutLockingRewardRows()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(
            campaignEvent,
            conditionJson: """{"sources":["REFERRAL"]}""");
        SetupContext(context);

        var result = await CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        result.Should().Be(new ProcessCustomerRegistrationCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Skipped,
            CampaignProcessingOutcomeCodes.ConditionNotMatched,
            1,
            0));
        _store.Verify(store => store.MarkSkipped(
            context.EventCampaignProcessingId,
            CampaignProcessingOutcomeCodes.ConditionNotMatched,
            Now.UtcDateTime), Times.Once);
        _store.Verify(store => store.LockCustomersAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(CampaignStatuses.Cancelled, CampaignSessionStatuses.Running)]
    [InlineData(CampaignStatuses.Active, CampaignSessionStatuses.Cancelled)]
    public async Task Handle_CancelledLifecycle_SkipsBeforeRewardQueries(
        string campaignStatus,
        string sessionStatus)
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(
            campaignEvent,
            campaignStatus: campaignStatus,
            sessionStatus: sessionStatus);
        SetupContext(context);

        var result = await CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        result.Should().Be(new ProcessCustomerRegistrationCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Skipped,
            CampaignProcessingOutcomeCodes.CampaignCancelled,
            1,
            0));
        _store.Verify(store => store.MarkSkipped(
            context.EventCampaignProcessingId,
            CampaignProcessingOutcomeCodes.CampaignCancelled,
            Now.UtcDateTime), Times.Once);
        _store.Verify(store => store.LockCustomersAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _store.Verify(store => store.LockActionsAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _store.Verify(store => store.ApplyPointReward(
            It.IsAny<PointRewardMutation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserTotalLimitReached_SkipsBeforePointLocks()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent, userLimitTotal: 1);
        SetupContext(context);
        SetupCustomerAndUsage(campaignEvent.CustomerId, totalUsed: 1);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateAction()]);

        var result = await CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        result.OutcomeCode.Should()
            .Be(CampaignProcessingOutcomeCodes.UserTotalLimitReached);
        _store.Verify(store => store.LockCustomerPointsAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActionSessionLimitReached_SkipsWithoutRewardMutation()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent);
        var action = CreateAction(sessionCount: 1);
        SetupEligibleContext(context, campaignEvent, [action]);
        _store.Setup(store => store.LockActionUsagesAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                context.CampaignSessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [action.ActionId] = 1 });

        var result = await CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        result.OutcomeCode.Should()
            .Be(CampaignProcessingOutcomeCodes.ActionSessionLimitReached);
        _store.Verify(store => store.ApplyPointReward(
            It.IsAny<PointRewardMutation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TwoEligibleActions_AppliesBothAndCompletesChild()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent, attemptCount: 2);
        var firstAction = CreateAction(executeOrder: 1, amount: 50);
        var secondAction = CreateAction(executeOrder: 2, amount: 100);
        SetupEligibleContext(
            context,
            campaignEvent,
            [firstAction, secondAction]);

        var result = await CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        result.Should().Be(new ProcessCustomerRegistrationCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Completed,
            null,
            3,
            2));
        _store.Verify(store => store.ApplyPointReward(
            It.Is<PointRewardMutation>(mutation =>
                mutation.EventCampaignProcessingId ==
                    context.EventCampaignProcessingId &&
                mutation.CustomerId == campaignEvent.CustomerId &&
                mutation.Amount == 50)), Times.Once);
        _store.Verify(store => store.ApplyPointReward(
            It.Is<PointRewardMutation>(mutation =>
                mutation.EventCampaignProcessingId ==
                    context.EventCampaignProcessingId &&
                mutation.CustomerId == campaignEvent.CustomerId &&
                mutation.Amount == 100)), Times.Once);
        _store.Verify(store => store.MarkCompleted(
            context.EventCampaignProcessingId,
            Now.UtcDateTime), Times.Once);
        _store.Verify(store => store.GetCampaignUsageCountsAsync(
            context.CampaignId,
            context.CampaignSessionId,
            campaignEvent.CustomerId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NormalEventWithReferrerAction_RejectsInvalidConfiguration()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent);
        var action = CreateAction(recipient: "REFERRER");
        SetupEligibleContext(context, campaignEvent, [action]);

        var act = () => CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<CampaignConfigurationException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignConfigurationInvalid);
        _store.Verify(store => store.ApplyPointReward(
            It.IsAny<PointRewardMutation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReferralEvent_ExecutesEventCustomerAndReferrerAtomically()
    {
        var referrerCustomerId = Guid.NewGuid();
        var campaignEvent = CreateEvent(
            CustomerRegistrationSources.Referral,
            referrerCustomerId);
        var context = CreateContext(
            campaignEvent,
            conditionJson: """{"sources":["REFERRAL"]}""");
        var customerAction = CreateAction(
            executeOrder: 1,
            amount: 50,
            recipient: "EVENT_CUSTOMER");
        var referrerAction = CreateAction(
            executeOrder: 2,
            amount: 100,
            recipient: "REFERRER");
        SetupContext(context);
        _store.Setup(store => store.LockCustomersAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([campaignEvent.CustomerId, referrerCustomerId]);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([customerAction, referrerAction]);
        _store.Setup(store => store.GetCampaignUsageCountsAsync(
                context.CampaignId,
                context.CampaignSessionId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampaignUsageCounts(0, 0));
        _store.Setup(store => store.LockCustomerPointsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _store.Setup(store => store.LockActionUsagesAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                context.CampaignSessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        result.Status.Should().Be(EventCampaignProcessingStatuses.Completed);
        result.ExecutedActionCount.Should().Be(2);
        var expectedLockOrder = new[]
        {
            campaignEvent.CustomerId,
            referrerCustomerId
        }.Order().ToArray();
        _store.Verify(store => store.LockCustomersAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids =>
                ids.SequenceEqual(expectedLockOrder)),
            It.IsAny<CancellationToken>()), Times.Once);
        _store.Verify(store => store.ApplyPointReward(
            It.Is<PointRewardMutation>(mutation =>
                mutation.ActionId == customerAction.ActionId &&
                mutation.CustomerId == campaignEvent.CustomerId &&
                mutation.Amount == 50)), Times.Once);
        _store.Verify(store => store.ApplyPointReward(
            It.Is<PointRewardMutation>(mutation =>
                mutation.ActionId == referrerAction.ActionId &&
                mutation.CustomerId == referrerCustomerId &&
                mutation.Amount == 100)), Times.Once);
        _store.Verify(store => store.GetCampaignUsageCountsAsync(
            context.CampaignId,
            context.CampaignSessionId,
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ReferralActionWithMissingReferrer_ThrowsPermanentFailure()
    {
        var referrerCustomerId = Guid.NewGuid();
        var campaignEvent = CreateEvent(
            CustomerRegistrationSources.Referral,
            referrerCustomerId);
        var context = CreateContext(
            campaignEvent,
            conditionJson: """{"sources":["REFERRAL"]}""");
        var action = CreateAction(recipient: "REFERRER");
        SetupContext(context);
        _store.Setup(store => store.LockCustomersAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([campaignEvent.CustomerId]);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([action]);

        var act = () => CreateHandler().Handle(
            new ProcessCustomerRegistrationCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<CampaignProcessingException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.ReferrerCustomerNotFound);
        exception.Which.Retriable.Should().BeFalse();
        _store.Verify(store => store.ApplyPointReward(
            It.IsAny<PointRewardMutation>()), Times.Never);
    }

    private ProcessCustomerRegistrationCampaignCommandHandler CreateHandler()
    {
        return new ProcessCustomerRegistrationCampaignCommandHandler(
            _store.Object,
            new CustomerAccountRegisteredEventValidator(),
            new CustomerAccountRegisteredConditionEvaluator(),
            new IssuePointActionConfigParser(),
            new FixedTimeProvider(Now));
    }

    private void SetupEligibleContext(
        CampaignRewardProcessingContext context,
        ValidatedCustomerAccountRegisteredEvent campaignEvent,
        IReadOnlyList<CampaignRewardAction> actions)
    {
        SetupContext(context);
        SetupCustomerAndUsage(campaignEvent.CustomerId);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);
        _store.Setup(store => store.LockCustomerPointsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _store.Setup(store => store.LockActionUsagesAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                context.CampaignSessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());
    }

    private void SetupContext(CampaignRewardProcessingContext context)
    {
        _store.Setup(store => store.LockProcessingAsync(
                context.EventCampaignProcessingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);
    }

    private void SetupCustomerAndUsage(
        Guid customerId,
        int totalUsed = 0,
        int sessionUsed = 0)
    {
        _store.Setup(store => store.LockCustomersAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([customerId]);
        _store.Setup(store => store.GetCampaignUsageCountsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampaignUsageCounts(totalUsed, sessionUsed));
    }

    private static CampaignRewardProcessingContext CreateContext(
        ValidatedCustomerAccountRegisteredEvent campaignEvent,
        string status = EventCampaignProcessingStatuses.Pending,
        int attemptCount = 0,
        string conditionJson = """{"sources":["NORMAL"]}""",
        int? userLimitTotal = null,
        int? userLimitSession = null,
        string campaignStatus = CampaignStatuses.Active,
        string sessionStatus = CampaignSessionStatuses.Running)
    {
        var campaignId = Guid.NewGuid();
        return new CampaignRewardProcessingContext(
            Guid.NewGuid(),
            campaignEvent.EventId,
            campaignId,
            Guid.NewGuid(),
            campaignEvent.CustomerId,
            status,
            attemptCount,
            null,
            campaignEvent.EventType,
            campaignEvent.RoutingKey,
            campaignEvent.OccurredAt,
            campaignEvent.NormalizedPayload,
            campaignEvent.PayloadHash,
            campaignEvent.EventType,
            conditionJson,
            campaignStatus,
            userLimitTotal,
            userLimitSession,
            campaignId,
            sessionStatus,
            campaignEvent.OccurredAt.AddHours(-1),
            campaignEvent.OccurredAt.AddHours(1));
    }

    private static CampaignRewardAction CreateAction(
        int executeOrder = 1,
        decimal amount = 50,
        string recipient = "EVENT_CUSTOMER",
        int? totalCount = null,
        int? sessionCount = null,
        int usedCount = 0)
    {
        return new CampaignRewardAction(
            Guid.NewGuid(),
            "ISSUE_POINT",
            $$"""
              {
                "calculationType": "FIXED_AMOUNT",
                "recipient": "{{recipient}}",
                "amount": {{amount}},
                "calculationBase": null,
                "percentage": null,
                "maximumPoints": null
              }
              """,
            executeOrder,
            totalCount,
            sessionCount,
            usedCount);
    }

    private static ValidatedCustomerAccountRegisteredEvent CreateEvent(
        string source = CustomerRegistrationSources.Normal,
        Guid? referrerCustomerId = null)
    {
        var eventId = Guid.NewGuid();
        var data = new CustomerAccountRegisteredData(
            Guid.NewGuid(),
            Guid.NewGuid(),
            source,
            referrerCustomerId);
        var envelope = new OutgoingEvent<CustomerAccountRegisteredData>(
            eventId,
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered,
            Now.UtcDateTime,
            data);
        var body = JsonSerializer.SerializeToUtf8Bytes(
            envelope,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return new CustomerAccountRegisteredEventValidator().Validate(
            body,
            eventId.ToString("D"),
            envelope.EventType,
            envelope.RoutingKey);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
