using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Handlers;
using Core.Requests;
using FluentAssertions;
using Moq;

namespace Core.Tests.Handlers;

public sealed class PublishOutboxMessageCommandHandlerTests
{
    private readonly DateTimeOffset _now = new(2026, 7, 27, 9, 0, 0, TimeSpan.Zero);
    private readonly Mock<IOutboxDispatchStore> _store = new();
    private readonly Mock<IEventPublisher> _publisher = new();

    [Fact]
    public async Task Handle_ConfirmedPublish_MarksMessagePublished()
    {
        var message = CreateMessage(attemptCount: 0);
        SetupMessage(message);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new PublishOutboxMessageCommand(message.EventId),
            CancellationToken.None);

        result.Outcome.Should().Be(OutboxDispatchOutcome.Published);
        _publisher.Verify(publisher => publisher.PublishAsync(
            It.Is<OutgoingMessage>(outgoing =>
                outgoing.EventId == message.EventId
                && outgoing.EventType == message.EventType
                && outgoing.RoutingKey == message.RoutingKey
                && outgoing.Body == message.Payload),
            CancellationToken.None), Times.Once);
        _store.Verify(store => store.MarkPublished(
            message.EventId,
            1,
            _now.UtcDateTime), Times.Once);
    }

    [Theory]
    [InlineData(0, 5, OutboxDispatchOutcome.Retried)]
    [InlineData(1, 10, OutboxDispatchOutcome.Retried)]
    [InlineData(2, 0, OutboxDispatchOutcome.Failed)]
    public async Task Handle_PublishFailure_RecordsAttemptAndExpectedBackoff(
        int existingAttempts,
        int retryDelaySeconds,
        OutboxDispatchOutcome expectedOutcome)
    {
        var message = CreateMessage(existingAttempts);
        SetupMessage(message);
        _publisher
            .Setup(publisher => publisher.PublishAsync(
                It.IsAny<OutgoingMessage>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OutboxPublishException(
                OutboxDispatchErrorCodes.BrokerUnreachable,
                " broker \n unavailable "));
        var handler = CreateHandler();

        var result = await handler.Handle(
            new PublishOutboxMessageCommand(message.EventId),
            CancellationToken.None);

        result.Outcome.Should().Be(expectedOutcome);
        _store.Verify(store => store.RecordFailure(
            message.EventId,
            existingAttempts + 1,
            expectedOutcome == OutboxDispatchOutcome.Failed ? "FAILED" : "PENDING",
            retryDelaySeconds == 0
                ? _now.UtcDateTime
                : _now.UtcDateTime.AddSeconds(retryDelaySeconds),
            OutboxDispatchErrorCodes.BrokerUnreachable,
            "broker unavailable"), Times.Once);
    }

    [Fact]
    public async Task Handle_CooperativeCancellation_RethrowsWithoutRecordingFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var message = CreateMessage(attemptCount: 0);
        SetupMessage(message);
        _publisher
            .Setup(publisher => publisher.PublishAsync(
                It.IsAny<OutgoingMessage>(),
                cancellation.Token))
            .Callback(cancellation.Cancel)
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));
        var handler = CreateHandler();

        var act = () => handler.Handle(
            new PublishOutboxMessageCommand(message.EventId),
            cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        _store.Verify(store => store.RecordFailure(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MessageNoLongerDispatchable_ReturnsSkipped()
    {
        var eventId = Guid.NewGuid();
        _store.Setup(store => store.GetDispatchableAsync(
                eventId,
                _now.UtcDateTime,
                OutboxDispatchConstants.MaxAttempts,
                CancellationToken.None))
            .ReturnsAsync((OutboxDispatchMessage?)null);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new PublishOutboxMessageCommand(eventId),
            CancellationToken.None);

        result.Outcome.Should().Be(OutboxDispatchOutcome.Skipped);
        _publisher.VerifyNoOtherCalls();
    }

    private PublishOutboxMessageCommandHandler CreateHandler()
    {
        return new PublishOutboxMessageCommandHandler(
            _store.Object,
            _publisher.Object,
            new FixedTimeProvider(_now));
    }

    private void SetupMessage(OutboxDispatchMessage message)
    {
        _store.Setup(store => store.GetDispatchableAsync(
                message.EventId,
                _now.UtcDateTime,
                OutboxDispatchConstants.MaxAttempts,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
    }

    private static OutboxDispatchMessage CreateMessage(int attemptCount)
    {
        return new OutboxDispatchMessage(
            Guid.NewGuid(),
            "CUSTOMER_ACCOUNT_REGISTERED",
            "customer.account.registered",
            """{"eventId":"test"}""",
            attemptCount);
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
