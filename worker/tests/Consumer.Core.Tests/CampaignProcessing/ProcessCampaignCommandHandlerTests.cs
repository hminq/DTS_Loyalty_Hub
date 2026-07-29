using System.Text.Json;
using Campaign.Contracts.Actions;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Handlers;
using Consumer.Core.Requests;
using Consumer.Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;

namespace Consumer.Core.Tests.CampaignProcessing;

public sealed class ProcessCampaignCommandHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<ICampaignRewardExecutionStore> _store = new();
    private readonly Mock<ICampaignActionExecutorRegistry> _executorRegistry = new();
    private readonly Mock<ICampaignActionExecutor> _executor = new();

    public ProcessCampaignCommandHandlerTests()
    {
        _executorRegistry
            .Setup(registry => registry.GetRequired(ActionTypes.IssuePoint))
            .Returns(_executor.Object);
        _executor
            .Setup(executor => executor.PrepareAsync(
                It.IsAny<IReadOnlyCollection<ResolvedCampaignAction>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_CompletedChild_ReturnsStoredOutcomeWithoutExecuting()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(
            campaignEvent,
            status: EventCampaignProcessingStatuses.Completed,
            attemptCount: 1);
        SetupContext(context);

        var result = await Handle(context);

        result.Status.Should().Be(EventCampaignProcessingStatuses.Completed);
        result.AttemptCount.Should().Be(1);
        result.ExecutedActionCount.Should().Be(0);
        _store.Verify(store => store.LockProcessingAsync(
            context.EventCampaignProcessingId,
            It.IsAny<CancellationToken>()), Times.Once);
        _store.VerifyNoOtherCalls();
        _executorRegistry.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Level2ConditionMismatch_SkipsBeforeLockingActions()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(
            campaignEvent,
            conditionJson: ConditionFor(CustomerRegistrationSources.Referral));
        SetupContext(context);

        var result = await Handle(context);

        result.OutcomeCode.Should()
            .Be(CampaignProcessingOutcomeCodes.ConditionNotMatched);
        _store.Verify(store => store.MarkSkipped(
            context.EventCampaignProcessingId,
            CampaignProcessingOutcomeCodes.ConditionNotMatched,
            Now.UtcDateTime), Times.Once);
        _store.Verify(store => store.LockActionsAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(CampaignStatuses.Cancelled, CampaignSessionStatuses.Running)]
    [InlineData(CampaignStatuses.Active, CampaignSessionStatuses.Cancelled)]
    public async Task Handle_CancelledLifecycle_SkipsBeforeActionPlanning(
        string campaignStatus,
        string sessionStatus)
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(
            campaignEvent,
            campaignStatus: campaignStatus,
            sessionStatus: sessionStatus);
        SetupContext(context);

        var result = await Handle(context);

        result.OutcomeCode.Should()
            .Be(CampaignProcessingOutcomeCodes.CampaignCancelled);
        _store.Verify(store => store.LockActionsAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _executorRegistry.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_UserTotalLimitReached_SkipsBeforeExecutorResourceLocks()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent, userLimitTotal: 1);
        var action = CreateAction();
        SetupEligibleContext(context, [action], [campaignEvent.CustomerId]);
        _store.Setup(store => store.GetCampaignUsageCountsAsync(
                context.CampaignId,
                context.CampaignSessionId,
                campaignEvent.CustomerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampaignUsageCounts(1, 0));

        var result = await Handle(context);

        result.OutcomeCode.Should()
            .Be(CampaignProcessingOutcomeCodes.UserTotalLimitReached);
        _executor.Verify(executor => executor.PrepareAsync(
            It.IsAny<IReadOnlyCollection<ResolvedCampaignAction>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActionSessionLimitReached_DoesNotExecuteOrRecordUsage()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent);
        var action = CreateAction(sessionCount: 1);
        SetupEligibleContext(context, [action], [campaignEvent.CustomerId]);
        _store.Setup(store => store.LockActionUsagesAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                context.CampaignSessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [action.ActionId] = 1 });

        var result = await Handle(context);

        result.OutcomeCode.Should()
            .Be(CampaignProcessingOutcomeCodes.ActionSessionLimitReached);
        _executor.Verify(executor => executor.Execute(
            It.IsAny<ResolvedCampaignAction>(),
            It.IsAny<CampaignActionExecutionContext>()), Times.Never);
        _store.Verify(store => store.RecordSuccessfulActionExecution(
            It.IsAny<SuccessfulActionExecutionMutation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TwoActions_ExecutesTypedPlansAndRecordsGenericUsageOnceEach()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent, attemptCount: 2);
        var firstAction = CreateAction(executeOrder: 1, amount: 50);
        var secondAction = CreateAction(executeOrder: 2, amount: 100);
        SetupEligibleContext(
            context,
            [firstAction, secondAction],
            [campaignEvent.CustomerId]);

        var result = await Handle(context);

        result.Should().Be(new ProcessCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Completed,
            null,
            3,
            2));
        _executor.Verify(executor => executor.Execute(
            It.Is<ResolvedCampaignAction>(action =>
                action.ActionId == firstAction.ActionId &&
                action.TargetId == campaignEvent.CustomerId &&
                HasAmount(action, 50)),
            It.IsAny<CampaignActionExecutionContext>()), Times.Once);
        _executor.Verify(executor => executor.Execute(
            It.Is<ResolvedCampaignAction>(action =>
                action.ActionId == secondAction.ActionId &&
                action.TargetId == campaignEvent.CustomerId &&
                HasAmount(action, 100)),
            It.IsAny<CampaignActionExecutionContext>()), Times.Once);
        _store.Verify(store => store.RecordSuccessfulActionExecution(
            It.IsAny<SuccessfulActionExecutionMutation>()), Times.Exactly(2));
        _store.Verify(store => store.MarkCompleted(
            context.EventCampaignProcessingId,
            Now.UtcDateTime), Times.Once);
    }

    [Fact]
    public async Task Handle_NormalEvent_ExcludesNonApplicableReferrerAction()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent, conditionJson: MatchAllCondition);
        var customerAction = CreateAction(
            executeOrder: 1,
            targetSelector: CustomerRegistrationTargetSelectors.EventCustomer);
        var referrerAction = CreateAction(
            executeOrder: 2,
            targetSelector: CustomerRegistrationTargetSelectors.Referrer);
        SetupEligibleContext(
            context,
            [customerAction, referrerAction],
            [campaignEvent.CustomerId]);

        var result = await Handle(context);

        result.ExecutedActionCount.Should().Be(1);
        _store.Verify(store => store.LockCustomersAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids =>
                ids.SequenceEqual(new[] { campaignEvent.CustomerId })),
            It.IsAny<CancellationToken>()), Times.Once);
        _store.Verify(store => store.LockActionUsagesAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids =>
                ids.SequenceEqual(new[] { customerAction.ActionId })),
            context.CampaignSessionId,
            It.IsAny<CancellationToken>()), Times.Once);
        _executor.Verify(executor => executor.Execute(
            It.Is<ResolvedCampaignAction>(action =>
                action.ActionId == customerAction.ActionId),
            It.IsAny<CampaignActionExecutionContext>()), Times.Once);
        _executor.Verify(executor => executor.Execute(
            It.Is<ResolvedCampaignAction>(action =>
                action.ActionId == referrerAction.ActionId),
            It.IsAny<CampaignActionExecutionContext>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AllActionsNotApplicable_PersistsStableSkip()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent, conditionJson: MatchAllCondition);
        var referrerAction = CreateAction(
            targetSelector: CustomerRegistrationTargetSelectors.Referrer);
        SetupContext(context);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([referrerAction]);

        var result = await Handle(context);

        result.Should().Be(new ProcessCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Skipped,
            CampaignProcessingOutcomeCodes.TargetNotApplicable,
            1,
            0));
        _store.Verify(store => store.MarkSkipped(
            context.EventCampaignProcessingId,
            CampaignProcessingOutcomeCodes.TargetNotApplicable,
            Now.UtcDateTime), Times.Once);
        _store.Verify(store => store.LockCustomersAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _executorRegistry.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ConditionAndTargetCannotOverlap_RejectsConfiguration()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(
            campaignEvent,
            conditionJson: ConditionFor(CustomerRegistrationSources.Normal));
        var referrerAction = CreateAction(
            targetSelector: CustomerRegistrationTargetSelectors.Referrer);
        SetupContext(context);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([referrerAction]);

        var act = () => Handle(context);

        var exception = await act.Should()
            .ThrowAsync<CampaignConfigurationException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
        _executorRegistry.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReferralEvent_ResolvesCustomerAndReferrerTargets()
    {
        var referrerCustomerId = Guid.NewGuid();
        var campaignEvent = CreateEvent(
            CustomerRegistrationSources.Referral,
            referrerCustomerId);
        var context = CreateContext(campaignEvent, conditionJson: MatchAllCondition);
        var customerAction = CreateAction(
            executeOrder: 1,
            amount: 50,
            targetSelector: CustomerRegistrationTargetSelectors.EventCustomer);
        var referrerAction = CreateAction(
            executeOrder: 2,
            amount: 100,
            targetSelector: CustomerRegistrationTargetSelectors.Referrer);
        var expectedLockOrder = new[]
        {
            campaignEvent.CustomerId,
            referrerCustomerId
        }.Order().ToArray();
        SetupEligibleContext(
            context,
            [customerAction, referrerAction],
            expectedLockOrder);

        var result = await Handle(context);

        result.ExecutedActionCount.Should().Be(2);
        _store.Verify(store => store.LockCustomersAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids =>
                ids.SequenceEqual(expectedLockOrder)),
            It.IsAny<CancellationToken>()), Times.Once);
        _executor.Verify(executor => executor.Execute(
            It.Is<ResolvedCampaignAction>(action =>
                action.ActionId == referrerAction.ActionId &&
                action.TargetId == referrerCustomerId &&
                HasAmount(action, 100)),
            It.IsAny<CampaignActionExecutionContext>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReferralTargetMissingFromDatabase_ThrowsPermanentFailure()
    {
        var referrerCustomerId = Guid.NewGuid();
        var campaignEvent = CreateEvent(
            CustomerRegistrationSources.Referral,
            referrerCustomerId);
        var context = CreateContext(campaignEvent, conditionJson: MatchAllCondition);
        var action = CreateAction(
            targetSelector: CustomerRegistrationTargetSelectors.Referrer);
        SetupContext(context);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([action]);
        _store.Setup(store => store.LockCustomersAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var act = () => Handle(context);

        var exception = await act.Should()
            .ThrowAsync<CampaignProcessingException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignTargetCustomerNotFound);
        exception.Which.Retriable.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_LegacyFlatActionConfig_RejectsConfiguration()
    {
        var campaignEvent = CreateEvent();
        var context = CreateContext(campaignEvent);
        var action = new CampaignRewardAction(
            Guid.NewGuid(),
            ActionTypes.IssuePoint,
            """{"recipient":"EVENT_CUSTOMER","amount":50}""",
            1,
            null,
            null,
            0);
        SetupContext(context);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([action]);

        var act = () => Handle(context);

        var exception = await act.Should()
            .ThrowAsync<CampaignConfigurationException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }

    private Task<ProcessCampaignResult> Handle(
        CampaignRewardProcessingContext context)
    {
        return CreateHandler().Handle(
            new ProcessCampaignCommand(
                context.EventCampaignProcessingId),
            CancellationToken.None);
    }

    private ProcessCampaignCommandHandler CreateHandler()
    {
        var validator = new CustomerAccountRegisteredEventValidator();
        var runtime = new CustomerAccountRegisteredEventRuntimeDefinition(validator);
        var runtimeRegistry = new CampaignEventRuntimeRegistry(
            CampaignDefinitionCatalog.BuiltIn,
            [runtime]);

        return new ProcessCampaignCommandHandler(
            _store.Object,
            runtimeRegistry,
            _executorRegistry.Object,
            CampaignDefinitionCatalog.BuiltIn,
            new Campaign.Contracts.Conditions.CampaignConditionParser(),
            new Campaign.Contracts.Conditions.CampaignConditionEvaluator(),
            new Campaign.Contracts.Conditions.CampaignConditionCompatibilityAnalyzer(),
            new CampaignActionBindingParser(),
            new FixedTimeProvider(Now));
    }

    private void SetupEligibleContext(
        CampaignRewardProcessingContext context,
        IReadOnlyList<CampaignRewardAction> actions,
        IReadOnlyList<Guid> lockedCustomerIds)
    {
        SetupContext(context);
        _store.Setup(store => store.LockActionsAsync(
                context.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);
        _store.Setup(store => store.LockCustomersAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockedCustomerIds);
        _store.Setup(store => store.GetCampaignUsageCountsAsync(
                context.CampaignId,
                context.CampaignSessionId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampaignUsageCounts(0, 0));
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

    private static CampaignRewardProcessingContext CreateContext(
        ValidatedCustomerAccountRegisteredEvent campaignEvent,
        string status = EventCampaignProcessingStatuses.Pending,
        int attemptCount = 0,
        string? conditionJson = null,
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
            conditionJson ?? ConditionFor(campaignEvent.Source),
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
        string targetSelector = CustomerRegistrationTargetSelectors.EventCustomer,
        int? totalCount = null,
        int? sessionCount = null,
        int usedCount = 0)
    {
        return new CampaignRewardAction(
            Guid.NewGuid(),
            ActionTypes.IssuePoint,
            CampaignActionBindingParser.ToCanonicalJson(
                targetSelector,
                new IssuePointParameters(amount)),
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
        var envelope = new OutgoingEvent<CustomerAccountRegisteredData>(
            eventId,
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered,
            Now.UtcDateTime,
            new CustomerAccountRegisteredData(
                Guid.NewGuid(),
                Guid.NewGuid(),
                source,
                referrerCustomerId));
        var body = JsonSerializer.SerializeToUtf8Bytes(
            envelope,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return new CustomerAccountRegisteredEventValidator().Validate(
            body,
            eventId.ToString("D"),
            envelope.EventType,
            envelope.RoutingKey);
    }

    private static string ConditionFor(string source) =>
        $$"""{"all":[{"field":"source","operator":"EQUALS","value":"{{source}}"}]}""";

    private const string MatchAllCondition = """{"all":[]}""";

    private static bool HasAmount(ResolvedCampaignAction action, decimal amount) =>
        action.ParsedParameters is IssuePointParameters parameters &&
        parameters.Amount == amount;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
