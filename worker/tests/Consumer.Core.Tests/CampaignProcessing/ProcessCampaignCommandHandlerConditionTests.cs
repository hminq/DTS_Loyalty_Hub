using System.Text.Json;
using Campaign.Contracts.Actions;
using Campaign.Contracts.Conditions;
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

public sealed class ProcessCampaignCommandHandlerConditionTests
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

    [Fact]
    public async Task Handle_ConditionMismatch_MarksSkippedWithConditionNotMatched()
    {
        var childId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var eventTypeVersionId = Guid.NewGuid();
        var eventTypeId = Guid.NewGuid();
        var occurredAt = Now.UtcDateTime;

        var fieldSchema = new EventPayloadFieldSchema("score", EventPayloadDataTypes.Number, null, true, true);
        var targetSchema = new EventTargetSchema("user", EventTargetKinds.Customer, "userId");
        var userIdFieldSchema = new EventPayloadFieldSchema("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, true, false);

        var definition = new PublishedEventDefinition(
            eventTypeId,
            eventTypeVersionId,
            "TEST_EVENT",
            "test.routing",
            1,
            "PUBLISHED",
            occurredAt,
            new EventPayloadSchema([userIdFieldSchema, fieldSchema], [targetSchema]));

        var payloadJson = $"{{\"userId\":\"{Guid.NewGuid():D}\",\"score\":10}}";
        using var payloadDoc = JsonDocument.Parse(payloadJson);

        var rawEnvelope = new RawEventEnvelope(
            eventId,
            "TEST_EVENT",
            1,
            occurredAt,
            payloadDoc.RootElement);

        _envelopeParser.Result = rawEnvelope;

        _definitionProvider.Setup(x => x.GetDefinitionAsync("TEST_EVENT", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, "test.routing");

        var context = new CampaignRewardProcessingContext(
            EventCampaignProcessingId: childId,
            EventId: eventId,
            CampaignId: campaignId,
            CampaignSessionId: sessionId,
            Status: EventCampaignProcessingStatuses.Pending,
            AttemptCount: 0,
            OutcomeCode: null,
            EventType: "TEST_EVENT",
            EventTypeVersionId: eventTypeVersionId,
            EventVersion: 1,
            RoutingKey: "test.routing",
            OccurredAt: occurredAt,
            NormalizedPayload: validatedEvent.NormalizedPayload,
            PayloadHash: validatedEvent.PayloadHash,
            CampaignEventTypeVersionId: eventTypeVersionId,
            CampaignConditionJson:
                "{\"all\":[{\"field\":\"score\",\"operator\":\"GT\",\"value\":50}]}",
            CampaignStatus: CampaignStatuses.Active,
            UserLimitTotal: null,
            UserLimitSession: null,
            SessionCampaignId: campaignId,
            SessionStatus: CampaignSessionStatuses.Running,
            SessionStart: occurredAt.AddHours(-1),
            SessionEnd: occurredAt.AddHours(1));

        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        result.Status.Should().Be(EventCampaignProcessingStatuses.Skipped);
        result.OutcomeCode.Should().Be(CampaignProcessingOutcomeCodes.ConditionNotMatched);

        _store.Verify(x => x.MarkSkipped(
            childId,
            CampaignProcessingOutcomeCodes.ConditionNotMatched,
            Now.UtcDateTime), Times.Once);
        _store.Verify(x => x.LockActionsAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _actionExecutorRegistry.Verify(x => x.GetRequired(
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidConditionConfiguration_ThrowsCampaignConfigurationException()
    {
        var childId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var eventTypeVersionId = Guid.NewGuid();
        var eventTypeId = Guid.NewGuid();
        var occurredAt = Now.UtcDateTime;

        var fieldSchema = new EventPayloadFieldSchema("score", EventPayloadDataTypes.Number, null, true, true);
        var targetSchema = new EventTargetSchema("user", EventTargetKinds.Customer, "userId");
        var userIdFieldSchema = new EventPayloadFieldSchema("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, true, false);

        var definition = new PublishedEventDefinition(
            eventTypeId,
            eventTypeVersionId,
            "TEST_EVENT",
            "test.routing",
            1,
            "PUBLISHED",
            occurredAt,
            new EventPayloadSchema([userIdFieldSchema, fieldSchema], [targetSchema]));

        var payloadJson = $"{{\"userId\":\"{Guid.NewGuid():D}\",\"score\":10}}";
        using var payloadDoc = JsonDocument.Parse(payloadJson);

        var rawEnvelope = new RawEventEnvelope(
            eventId,
            "TEST_EVENT",
            1,
            occurredAt,
            payloadDoc.RootElement);

        _envelopeParser.Result = rawEnvelope;

        _definitionProvider.Setup(x => x.GetDefinitionAsync("TEST_EVENT", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        var validatedEvent = _eventFactory.Create(rawEnvelope, definition, "test.routing");

        // Invalid condition: string value for number field (coercion rejected)
        var context = new CampaignRewardProcessingContext(
            EventCampaignProcessingId: childId,
            EventId: eventId,
            CampaignId: campaignId,
            CampaignSessionId: sessionId,
            Status: EventCampaignProcessingStatuses.Pending,
            AttemptCount: 0,
            OutcomeCode: null,
            EventType: "TEST_EVENT",
            EventTypeVersionId: eventTypeVersionId,
            EventVersion: 1,
            RoutingKey: "test.routing",
            OccurredAt: occurredAt,
            NormalizedPayload: validatedEvent.NormalizedPayload,
            PayloadHash: validatedEvent.PayloadHash,
            CampaignEventTypeVersionId: eventTypeVersionId,
            CampaignConditionJson:
                "{\"all\":[{\"field\":\"score\",\"operator\":\"EQUALS\",\"value\":\"10\"}]}",
            CampaignStatus: CampaignStatuses.Active,
            UserLimitTotal: null,
            UserLimitSession: null,
            SessionCampaignId: campaignId,
            SessionStatus: CampaignSessionStatuses.Running,
            SessionStart: occurredAt.AddHours(-1),
            SessionEnd: occurredAt.AddHours(1));

        _store.Setup(x => x.LockProcessingAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var handler = CreateHandler();

        var act = () => handler.Handle(
            new ProcessCampaignCommand(childId),
            CancellationToken.None);

        await act.Should().ThrowAsync<CampaignConfigurationException>()
            .WithMessage("*" + CampaignProcessingErrorCodes.CampaignConfigurationInvalid + "*");

        _store.Verify(x => x.MarkSkipped(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Never);
        _store.Verify(x => x.LockActionsAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _actionExecutorRegistry.Verify(x => x.GetRequired(
            It.IsAny<string>()), Times.Never);
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
