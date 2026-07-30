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

public sealed class ProcessCampaignCommandHandlerPinningTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 30, 11, 0, 0, TimeSpan.Zero);

    private readonly Mock<ICampaignRewardExecutionStore> _store = new();
    private readonly StubVersionedEnvelopeParser _envelopeParser = new();
    private readonly Mock<IEventDefinitionProvider> _definitionProvider = new();
    private readonly GenericCampaignEventFactory _eventFactory = new();
    private readonly GenericCampaignTargetResolver _targetResolver = new();
    private readonly Mock<ICampaignActionExecutorRegistry> _actionExecutorRegistry = new();
    private readonly GenericCampaignConditionEvaluator _conditionEvaluator = new();
    private readonly CampaignActionBindingParser _actionBindingParser = new();

    [Fact]
    public async Task Handle_ValidPinnedGenericEvent_UsesExactDefinitionAndCompletes()
    {
        var childId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var definition = CreateDefinition("DISPOSABLE_EVENT", "disposable.routing");
        var rawEnvelope = CreateEnvelope(Guid.NewGuid(), definition, customerId);
        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, definition.RoutingKey);

        _envelopeParser.Result = rawEnvelope;
        _definitionProvider.Setup(x => x.GetDefinitionAsync(
                definition.EventTypeCode,
                definition.Version,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);
        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateContext(childId, definition, validatedEvent));
        _store.Setup(x => x.LockActionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Action(Guid.NewGuid(), "audience", 1)]);
        _store.Setup(x => x.LockCustomersAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
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

        var executor = new Mock<ICampaignActionExecutor>();
        executor.SetupGet(x => x.ActionType).Returns(ActionTypes.IssuePoint);
        executor.SetupGet(x => x.RequiredTargetKind).Returns(CampaignTargetKinds.Customer);
        executor.Setup(x => x.PrepareAsync(
                It.IsAny<IReadOnlyCollection<ResolvedCampaignAction>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _actionExecutorRegistry
            .Setup(x => x.GetRequired(ActionTypes.IssuePoint))
            .Returns(executor.Object);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        result.Status.Should().Be(EventCampaignProcessingStatuses.Completed);
        _definitionProvider.Verify(x => x.GetDefinitionAsync(
            definition.EventTypeCode,
            definition.Version,
            It.IsAny<CancellationToken>()), Times.Once);
        _store.Verify(x => x.RecordSuccessfulActionExecution(
            It.Is<SuccessfulActionExecutionMutation>(mutation =>
                mutation.CustomerId == customerId)), Times.Once);
        _store.Verify(x => x.MarkCompleted(childId, Now.UtcDateTime), Times.Once);
    }

    [Fact]
    public async Task Handle_TerminalChild_ReturnsStoredResultWithoutDefinitionOrMutation()
    {
        var childId = Guid.NewGuid();
        var context = CreateContext(
            childId,
            CreateDefinition(),
            null,
            status: EventCampaignProcessingStatuses.Completed,
            outcomeCode: null);

        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var result = await CreateHandler().Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        result.Status.Should().Be(EventCampaignProcessingStatuses.Completed);
        _definitionProvider.Verify(x => x.GetDefinitionAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _store.Verify(x => x.LockActionsAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _store.Verify(x => x.RecordSuccessfulActionExecution(
            It.IsAny<SuccessfulActionExecutionMutation>()), Times.Never);
    }

    [Theory]
    [InlineData("eventId")]
    [InlineData("eventType")]
    [InlineData("eventVersion")]
    [InlineData("routingKey")]
    [InlineData("occurredAt")]
    [InlineData("payloadHash")]
    public async Task Handle_PersistedEventInvariantMismatch_FailsBeforeActionOrCustomerLocks(
        string mismatch)
    {
        var childId = Guid.NewGuid();
        var definition = CreateDefinition();
        var rawEnvelope = CreateEnvelope(Guid.NewGuid(), definition, Guid.NewGuid());
        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, definition.RoutingKey);
        var context = CreateContext(childId, definition, validatedEvent);

        context = mismatch switch
        {
            "eventId" => context with { EventId = Guid.NewGuid() },
            "eventType" => context with { EventType = "OTHER_EVENT" },
            "eventVersion" => context with { EventVersion = 2 },
            "routingKey" => context with { RoutingKey = "other.routing" },
            "occurredAt" => context with { OccurredAt = validatedEvent.OccurredAt.AddMicroseconds(1) },
            "payloadHash" => context with { PayloadHash = "different-hash" },
            _ => throw new InvalidOperationException("Unknown mismatch.")
        };

        _envelopeParser.Result = rawEnvelope;
        _definitionProvider.Setup(x => x.GetDefinitionAsync(
                definition.EventTypeCode,
                definition.Version,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);
        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var act = () => CreateHandler().Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<CampaignProcessingException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed);
        exception.Which.Retriable.Should().BeFalse();

        VerifyNoRewardSideEffects();
    }

    [Theory]
    [InlineData("campaignVersion")]
    [InlineData("sessionOwner")]
    [InlineData("sessionWindow")]
    public async Task Handle_PinnedConfigurationMismatch_FailsBeforeActionOrCustomerLocks(
        string mismatch)
    {
        var childId = Guid.NewGuid();
        var definition = CreateDefinition();
        var rawEnvelope = CreateEnvelope(Guid.NewGuid(), definition, Guid.NewGuid());
        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, definition.RoutingKey);
        var context = CreateContext(childId, definition, validatedEvent);

        context = mismatch switch
        {
            "campaignVersion" => context with { CampaignEventTypeVersionId = Guid.NewGuid() },
            "sessionOwner" => context with { SessionCampaignId = Guid.NewGuid() },
            "sessionWindow" => context with { SessionEnd = validatedEvent.OccurredAt },
            _ => throw new InvalidOperationException("Unknown mismatch.")
        };

        _envelopeParser.Result = rawEnvelope;
        _definitionProvider.Setup(x => x.GetDefinitionAsync(
                definition.EventTypeCode,
                definition.Version,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);
        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var act = () => CreateHandler().Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        await act.Should().ThrowAsync<CampaignConfigurationException>()
            .WithMessage(CampaignProcessingErrorCodes.CampaignConfigurationInvalid);

        VerifyNoRewardSideEffects();
    }

    [Theory]
    [InlineData("campaign")]
    [InlineData("session")]
    public async Task Handle_InvalidPinnedLifecycle_MapsToConfigurationFailureBeforeRewardWork(
        string invalidLifecycle)
    {
        var childId = Guid.NewGuid();
        var definition = CreateDefinition();
        var rawEnvelope = CreateEnvelope(Guid.NewGuid(), definition, Guid.NewGuid());
        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, definition.RoutingKey);
        var context = CreateContext(childId, definition, validatedEvent);

        context = invalidLifecycle switch
        {
            "campaign" => context with { CampaignStatus = CampaignStatuses.Draft },
            "session" => context with { SessionStatus = "UNKNOWN" },
            _ => throw new InvalidOperationException("Unknown lifecycle.")
        };

        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var act = () => CreateHandler().Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        await act.Should().ThrowAsync<CampaignConfigurationException>()
            .WithMessage(CampaignProcessingErrorCodes.CampaignConfigurationInvalid);

        _definitionProvider.Verify(x => x.GetDefinitionAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
        VerifyNoRewardSideEffects();
    }

    [Fact]
    public async Task Handle_MalformedPersistedEnvelope_MapsToNonRetriablePersistenceFailure()
    {
        var childId = Guid.NewGuid();
        var definition = CreateDefinition();
        var context = CreateContext(childId, definition, null);
        var parseException = new EventEnvelopeParseException(
            CampaignProcessingErrorCodes.EventPayloadInvalid);
        _envelopeParser.Exception = parseException;
        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var act = () => CreateHandler().Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<CampaignProcessingException>();
        exception.Which.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed);
        exception.Which.Retriable.Should().BeFalse();
        exception.Which.InnerException.Should().BeSameAs(parseException);

        VerifyNoRewardSideEffects();
    }

    [Fact]
    public async Task Handle_MissingExactDefinition_MapsToConfigurationFailureBeforeActionLocks()
    {
        var childId = Guid.NewGuid();
        var definition = CreateDefinition();
        var rawEnvelope = CreateEnvelope(Guid.NewGuid(), definition, Guid.NewGuid());
        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, definition.RoutingKey);

        _envelopeParser.Result = rawEnvelope;
        _definitionProvider.Setup(x => x.GetDefinitionAsync(
                definition.EventTypeCode,
                definition.Version,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublishedEventDefinition?)null);
        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateContext(childId, definition, validatedEvent));

        var act = () => CreateHandler().Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        await act.Should().ThrowAsync<CampaignConfigurationException>()
            .WithMessage(CampaignProcessingErrorCodes.CampaignConfigurationInvalid);

        VerifyNoRewardSideEffects();
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

    private void VerifyNoRewardSideEffects()
    {
        _store.Verify(x => x.LockActionsAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _store.Verify(x => x.LockCustomersAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _actionExecutorRegistry.Verify(x => x.GetRequired(
            It.IsAny<string>()), Times.Never);
        _store.Verify(x => x.RecordSuccessfulActionExecution(
            It.IsAny<SuccessfulActionExecutionMutation>()), Times.Never);
        _store.Verify(x => x.MarkCompleted(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>()), Times.Never);
    }

    private static PublishedEventDefinition CreateDefinition(
        string eventCode = "GENERIC_EVENT",
        string routingKey = "generic.event")
    {
        var customerIdField = new EventPayloadFieldSchema(
            "customerId",
            EventPayloadDataTypes.String,
            EventPayloadFormats.Uuid,
            true,
            false);

        return new PublishedEventDefinition(
            Guid.NewGuid(),
            Guid.NewGuid(),
            eventCode,
            routingKey,
            1,
            "PUBLISHED",
            Now.UtcDateTime,
            new EventPayloadSchema(
                [customerIdField],
                [new EventTargetSchema("audience", EventTargetKinds.Customer, "customerId")]));
    }

    private static RawEventEnvelope CreateEnvelope(
        Guid eventId,
        PublishedEventDefinition definition,
        Guid customerId)
    {
        using var payloadDocument = JsonDocument.Parse(
            $$"""{"customerId":"{{customerId:D}}"}""");
        return new RawEventEnvelope(
            eventId,
            definition.EventTypeCode,
            definition.Version,
            Now.UtcDateTime,
            payloadDocument.RootElement.Clone());
    }

    private static CampaignRewardProcessingContext CreateContext(
        Guid childId,
        PublishedEventDefinition definition,
        GenericValidatedCampaignEvent? validatedEvent,
        string status = EventCampaignProcessingStatuses.Pending,
        string? outcomeCode = null)
    {
        var campaignId = Guid.NewGuid();
        var eventId = validatedEvent?.EventId ?? Guid.NewGuid();
        var occurredAt = validatedEvent?.OccurredAt ?? Now.UtcDateTime;
        return new CampaignRewardProcessingContext(
            EventCampaignProcessingId: childId,
            EventId: eventId,
            CampaignId: campaignId,
            CampaignSessionId: Guid.NewGuid(),
            Status: status,
            AttemptCount: 0,
            OutcomeCode: outcomeCode,
            EventType: definition.EventTypeCode,
            EventTypeVersionId: definition.EventTypeVersionId,
            EventVersion: definition.Version,
            RoutingKey: definition.RoutingKey,
            OccurredAt: occurredAt,
            NormalizedPayload: validatedEvent?.NormalizedPayload ?? "{}",
            PayloadHash: validatedEvent?.PayloadHash ?? "hash",
            CampaignEventTypeVersionId: definition.EventTypeVersionId,
            CampaignConditionJson: "{\"all\":[]}",
            CampaignStatus: CampaignStatuses.Active,
            UserLimitTotal: null,
            UserLimitSession: null,
            SessionCampaignId: campaignId,
            SessionStatus: CampaignSessionStatuses.Running,
            SessionStart: occurredAt.AddHours(-1),
            SessionEnd: occurredAt.AddHours(1));
    }

    private static CampaignRewardAction Action(
        Guid actionId,
        string selector,
        int executeOrder)
    {
        return new CampaignRewardAction(
            actionId,
            ActionTypes.IssuePoint,
            CampaignActionBindingParser.ToCanonicalJson(
                selector,
                new IssuePointParameters(5m)),
            executeOrder,
            TotalCount: null,
            SessionCount: null,
            UsedCount: 0);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class StubVersionedEnvelopeParser : IVersionedEnvelopeParser
    {
        public RawEventEnvelope? Result { get; set; }

        public Exception? Exception { get; set; }

        public RawEventEnvelope Parse(
            ReadOnlySpan<byte> bodyJson,
            string? amqpMessageId,
            string? amqpType)
        {
            if (Exception is not null)
            {
                throw Exception;
            }

            return Result ?? throw new InvalidOperationException(
                "Test envelope result was not configured.");
        }
    }
}
