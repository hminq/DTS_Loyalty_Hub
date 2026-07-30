using System.Text.Json;
using Campaign.Contracts.Actions;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Entities.Definitions;
using Consumer.Core.Entities.Envelope;
using Consumer.Core.Exceptions;
using Consumer.Core.Handlers;
using Consumer.Core.Requests;
using Consumer.Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;

namespace Consumer.Core.Tests.CampaignProcessing;

public sealed class ProcessCampaignCommandHandlerTargetTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<ICampaignRewardExecutionStore> _store = new();
    private readonly StubVersionedEnvelopeParser _envelopeParser = new();
    private readonly Mock<IEventDefinitionProvider> _definitionProvider = new();
    private readonly GenericCampaignEventFactory _eventFactory = new();
    private readonly GenericCampaignTargetResolver _targetResolver = new();
    private readonly Mock<ICampaignActionExecutorRegistry> _actionExecutorRegistry = new();
    private readonly GenericCampaignConditionEvaluator _conditionEvaluator = new();
    private readonly CampaignActionBindingParser _actionBindingParser = new();

    [Fact]
    public async Task Handle_TwoActions_PassesExactSelectorKindAndTargetIdToExecutors()
    {
        var childId = Guid.NewGuid();
        var actionId1 = Guid.NewGuid();
        var actionId2 = Guid.NewGuid();
        var registeredCustomerId = Guid.NewGuid();
        var referrerCustomerId = Guid.NewGuid();
        var definition = CreateDefinition();
        var rawEnvelope = CreateEnvelope(
            childEventId: Guid.NewGuid(),
            definition,
            registeredCustomerId,
            referrerCustomerId);
        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, definition.RoutingKey);
        _envelopeParser.Result = rawEnvelope;
        _definitionProvider
            .Setup(x => x.GetDefinitionAsync(definition.EventTypeCode, definition.Version, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateContext(childId, rawEnvelope.EventId, definition, validatedEvent));
        _store.Setup(x => x.LockActionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                Action(actionId1, "registeredCustomer", executeOrder: 2, amount: 5m),
                Action(actionId2, "referrerCustomer", executeOrder: 1, amount: 7m)
            ]);

        IReadOnlyCollection<Guid>? lockedCustomerIds = null;
        _store.Setup(x => x.LockCustomersAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<Guid>, CancellationToken>((ids, _) => lockedCustomerIds = ids.ToArray())
            .ReturnsAsync((IReadOnlyCollection<Guid> ids, CancellationToken _) => ids.ToArray());
        _store.Setup(x => x.GetCampaignUsageCountsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampaignUsageCounts(0, 0));
        _store.Setup(x => x.LockActionUsagesAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var preparedActions = Array.Empty<ResolvedCampaignAction>();
        var executedActions = new List<ResolvedCampaignAction>();
        var executor = new Mock<ICampaignActionExecutor>();
        executor.SetupGet(x => x.ActionType).Returns(ActionTypes.IssuePoint);
        executor.SetupGet(x => x.RequiredTargetKind).Returns(CampaignTargetKinds.Customer);
        executor.Setup(x => x.PrepareAsync(
                It.IsAny<IReadOnlyCollection<ResolvedCampaignAction>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<ResolvedCampaignAction>, CancellationToken>(
                (actions, _) => preparedActions = actions.ToArray())
            .Returns(Task.CompletedTask);
        executor.Setup(x => x.Execute(
                It.IsAny<ResolvedCampaignAction>(),
                It.IsAny<CampaignActionExecutionContext>()))
            .Callback<ResolvedCampaignAction, CampaignActionExecutionContext>(
                (action, _) => executedActions.Add(action));
        _actionExecutorRegistry
            .Setup(x => x.GetRequired(ActionTypes.IssuePoint))
            .Returns(executor.Object);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        result.Status.Should().Be(EventCampaignProcessingStatuses.Completed);
        lockedCustomerIds.Should().Equal(
            new[] { registeredCustomerId, referrerCustomerId }.Order());

        preparedActions.Should().HaveCount(2);
        preparedActions.Should().ContainSingle(action =>
            action.ActionId == actionId1 &&
            action.TargetSelector == "registeredCustomer" &&
            action.TargetKind == CampaignTargetKinds.Customer &&
            action.TargetId == registeredCustomerId);
        preparedActions.Should().ContainSingle(action =>
            action.ActionId == actionId2 &&
            action.TargetSelector == "referrerCustomer" &&
            action.TargetKind == CampaignTargetKinds.Customer &&
            action.TargetId == referrerCustomerId);

        executedActions.Select(action => action.ActionId)
            .Should().Equal(actionId2, actionId1);
        _store.Verify(x => x.RecordSuccessfulActionExecution(
            It.Is<SuccessfulActionExecutionMutation>(mutation =>
                mutation.ActionId == actionId1 &&
                mutation.CustomerId == registeredCustomerId)), Times.Once);
        _store.Verify(x => x.RecordSuccessfulActionExecution(
            It.Is<SuccessfulActionExecutionMutation>(mutation =>
                mutation.ActionId == actionId2 &&
                mutation.CustomerId == referrerCustomerId)), Times.Once);
    }

    [Theory]
    [InlineData("RegisteredCustomer")]
    [InlineData(" registeredCustomer")]
    [InlineData("registeredCustomer ")]
    public async Task Handle_NonExactSelector_ThrowsInvalidActionConfigurationBeforeSideEffects(
        string selector)
    {
        var childId = Guid.NewGuid();
        var definition = CreateDefinition();
        var rawEnvelope = CreateEnvelope(
            childEventId: Guid.NewGuid(),
            definition,
            Guid.NewGuid(),
            Guid.NewGuid());
        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, definition.RoutingKey);
        _envelopeParser.Result = rawEnvelope;
        _definitionProvider
            .Setup(x => x.GetDefinitionAsync(definition.EventTypeCode, definition.Version, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateContext(childId, rawEnvelope.EventId, definition, validatedEvent));
        _store.Setup(x => x.LockActionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Action(Guid.NewGuid(), selector, executeOrder: 1, amount: 5m)]);

        var handler = CreateHandler();

        var act = () => handler.Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        await act.Should().ThrowAsync<CampaignConfigurationException>()
            .WithMessage("*" + CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid + "*");

        _store.Verify(x => x.LockCustomersAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _actionExecutorRegistry.Verify(x => x.GetRequired(
            It.IsAny<string>()), Times.Never);
        _store.Verify(x => x.RecordSuccessfulActionExecution(
            It.IsAny<SuccessfulActionExecutionMutation>()), Times.Never);
    }

    private ProcessCampaignCommandHandler CreateHandler()
    {
        return new ProcessCampaignCommandHandler(
            _store.Object,
            _envelopeParser,
            _definitionProvider.Object,
            _eventFactory,
            _targetResolver,
            _actionExecutorRegistry.Object,
            CampaignActionCatalog.BuiltIn,
            _conditionEvaluator,
            _actionBindingParser,
            new FixedTimeProvider(Now));
    }

    private static PublishedEventDefinition CreateDefinition()
    {
        var registeredCustomerIdField = new EventPayloadFieldSchema(
            "registeredCustomerId",
            EventPayloadDataTypes.String,
            EventPayloadFormats.Uuid,
            true,
            false);
        var referrerCustomerIdField = new EventPayloadFieldSchema(
            "referrerCustomerId",
            EventPayloadDataTypes.String,
            EventPayloadFormats.Uuid,
            true,
            false);

        return new PublishedEventDefinition(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "GENERIC_CUSTOMER_EVENT",
            "generic.customer.event",
            1,
            "PUBLISHED",
            Now.UtcDateTime,
            new EventPayloadSchema(
                [registeredCustomerIdField, referrerCustomerIdField],
                [
                    new EventTargetSchema(
                        "registeredCustomer",
                        EventTargetKinds.Customer,
                        "registeredCustomerId"),
                    new EventTargetSchema(
                        "referrerCustomer",
                        EventTargetKinds.Customer,
                        "referrerCustomerId")
                ]));
    }

    private static RawEventEnvelope CreateEnvelope(
        Guid childEventId,
        PublishedEventDefinition definition,
        Guid registeredCustomerId,
        Guid referrerCustomerId)
    {
        var payloadJson = $$"""
            {
              "registeredCustomerId": "{{registeredCustomerId:D}}",
              "referrerCustomerId": "{{referrerCustomerId:D}}"
            }
            """;
        using var payloadDocument = JsonDocument.Parse(payloadJson);

        return new RawEventEnvelope(
            childEventId,
            definition.EventTypeCode,
            definition.Version,
            Now.UtcDateTime,
            payloadDocument.RootElement.Clone());
    }

    private static CampaignRewardAction Action(
        Guid actionId,
        string selector,
        int executeOrder,
        decimal amount)
    {
        return new CampaignRewardAction(
            actionId,
            ActionTypes.IssuePoint,
            CampaignActionBindingParser.ToCanonicalJson(
                selector,
                new IssuePointParameters(amount)),
            executeOrder,
            TotalCount: null,
            SessionCount: null,
            UsedCount: 0);
    }

    private static CampaignRewardProcessingContext CreateContext(
        Guid childId,
        Guid eventId,
        PublishedEventDefinition definition,
        GenericValidatedCampaignEvent validatedEvent)
    {
        var campaignId = Guid.NewGuid();
        return new CampaignRewardProcessingContext(
            EventCampaignProcessingId: childId,
            EventId: eventId,
            CampaignId: campaignId,
            CampaignSessionId: Guid.NewGuid(),
            Status: EventCampaignProcessingStatuses.Pending,
            AttemptCount: 0,
            OutcomeCode: null,
            EventType: definition.EventTypeCode,
            EventTypeVersionId: definition.EventTypeVersionId,
            EventVersion: definition.Version,
            RoutingKey: definition.RoutingKey,
            OccurredAt: validatedEvent.OccurredAt,
            NormalizedPayload: validatedEvent.NormalizedPayload,
            PayloadHash: validatedEvent.PayloadHash,
            CampaignEventTypeVersionId: definition.EventTypeVersionId,
            CampaignConditionJson: "{\"all\":[]}",
            CampaignStatus: CampaignStatuses.Active,
            UserLimitTotal: null,
            UserLimitSession: null,
            SessionCampaignId: campaignId,
            SessionStatus: CampaignSessionStatuses.Running,
            SessionStart: validatedEvent.OccurredAt.AddHours(-1),
            SessionEnd: validatedEvent.OccurredAt.AddHours(1));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class StubVersionedEnvelopeParser : IVersionedEnvelopeParser
    {
        public RawEventEnvelope? Result { get; set; }

        public RawEventEnvelope Parse(
            ReadOnlySpan<byte> bodyJson,
            string? amqpMessageId,
            string? amqpType)
        {
            return Result ?? throw new InvalidOperationException(
                "Test envelope result was not configured.");
        }
    }
}
