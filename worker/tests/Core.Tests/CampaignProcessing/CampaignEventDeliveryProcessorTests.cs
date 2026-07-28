using System.Text.Json;
using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;

namespace Core.Tests.CampaignProcessing;

public sealed class CampaignEventDeliveryProcessorTests
{
    private static readonly DateTime OccurredAt =
        new(2026, 7, 28, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ICampaignProcessingScopeExecutor> _executor = new();

    [Fact]
    public async Task Process_InvalidEnvelope_RejectsWithoutDatabaseScope()
    {
        var processor = CreateProcessor();

        var result = await processor.ProcessAsync(
            new CampaignEventDelivery(
                "{}"u8.ToArray(),
                null,
                null,
                "wrong.routing.key",
                false),
            CancellationToken.None);

        result.Disposition.Should()
            .Be(CampaignEventDeliveryDispositions.Reject);
        result.EventId.Should().BeNull();
        result.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.EventPayloadInvalid);
        _executor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Process_ZeroCandidateEvent_Acknowledges()
    {
        var delivery = CreateDelivery();
        var validatedEvent = Validate(delivery);
        _executor.Setup(item => item.PrepareEventAsync(
                It.Is<ValidatedCustomerAccountRegisteredEvent>(campaignEvent =>
                    campaignEvent.EventId == validatedEvent.EventId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PrepareCustomerAccountRegisteredEventResult(
                validatedEvent.EventId,
                EventProcessingStatuses.Completed,
                Created: true,
                []));
        var processor = CreateProcessor();

        var result = await processor.ProcessAsync(
            delivery,
            CancellationToken.None);

        result.Disposition.Should()
            .Be(CampaignEventDeliveryDispositions.Acknowledge);
        result.PinnedCampaignCount.Should().Be(0);
        _executor.Verify(item => item.ProcessCampaignAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _executor.Verify(item => item.FinalizeEventAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Process_CompletedRedelivery_AcknowledgesWithoutRepeatingChild()
    {
        var delivery = CreateDelivery(redelivered: true);
        var validatedEvent = Validate(delivery);
        var target = Target(EventCampaignProcessingStatuses.Completed);
        _executor.Setup(item => item.PrepareEventAsync(
                It.IsAny<ValidatedCustomerAccountRegisteredEvent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PrepareCustomerAccountRegisteredEventResult(
                validatedEvent.EventId,
                EventProcessingStatuses.Completed,
                Created: false,
                [target]));
        var processor = CreateProcessor();

        var result = await processor.ProcessAsync(
            delivery,
            CancellationToken.None);

        result.Disposition.Should()
            .Be(CampaignEventDeliveryDispositions.Acknowledge);
        result.CompletedCampaignCount.Should().Be(1);
        _executor.Verify(item => item.ProcessCampaignAsync(
            target.EventCampaignProcessingId,
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Process_AllCampaignsCompleted_Acknowledges()
    {
        var delivery = CreateDelivery();
        var validatedEvent = Validate(delivery);
        var target = Target(EventCampaignProcessingStatuses.Pending);
        SetupPendingPreparation(validatedEvent.EventId, target);
        _executor.Setup(item => item.ProcessCampaignAsync(
                target.EventCampaignProcessingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessCustomerRegistrationCampaignResult(
                target.EventCampaignProcessingId,
                EventCampaignProcessingStatuses.Completed,
                null,
                1,
                1));
        _executor.Setup(item => item.FinalizeEventAsync(
                validatedEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FinalizeEventProcessingResult(
                validatedEvent.EventId,
                EventProcessingStatuses.Completed,
                1,
                0,
                0,
                0,
                Updated: true));
        var processor = CreateProcessor();

        var result = await processor.ProcessAsync(
            delivery,
            CancellationToken.None);

        result.Disposition.Should()
            .Be(CampaignEventDeliveryDispositions.Acknowledge);
        result.CompletedCampaignCount.Should().Be(1);
        result.FailedCampaignCount.Should().Be(0);
    }

    [Fact]
    public async Task Process_FailedCampaign_IsRecordedThenRejects()
    {
        var delivery = CreateDelivery();
        var validatedEvent = Validate(delivery);
        var target = Target(EventCampaignProcessingStatuses.Pending);
        SetupPendingPreparation(validatedEvent.EventId, target);
        _executor.Setup(item => item.ProcessCampaignAsync(
                target.EventCampaignProcessingId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CampaignConfigurationException(
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid));
        _executor.Setup(item => item.RecordFailureAsync(
                target.EventCampaignProcessingId,
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RecordCampaignProcessingFailureResult(
                target.EventCampaignProcessingId,
                EventCampaignProcessingStatuses.Failed,
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid,
                1,
                Updated: true));
        _executor.Setup(item => item.FinalizeEventAsync(
                validatedEvent.EventId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FinalizeEventProcessingResult(
                validatedEvent.EventId,
                EventProcessingStatuses.Failed,
                0,
                0,
                1,
                0,
                Updated: true));
        var processor = CreateProcessor();

        var result = await processor.ProcessAsync(
            delivery,
            CancellationToken.None);

        result.Disposition.Should()
            .Be(CampaignEventDeliveryDispositions.Reject);
        result.FailedCampaignCount.Should().Be(1);
    }

    [Fact]
    public async Task Process_EventIdCollision_RejectsWithoutProcessingCampaign()
    {
        var delivery = CreateDelivery();
        _executor.Setup(item => item.PrepareEventAsync(
                It.IsAny<ValidatedCustomerAccountRegisteredEvent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventIdCollision));
        var processor = CreateProcessor();

        var result = await processor.ProcessAsync(
            delivery,
            CancellationToken.None);

        result.Disposition.Should()
            .Be(CampaignEventDeliveryDispositions.Reject);
        result.ErrorCode.Should()
            .Be(CampaignProcessingErrorCodes.EventIdCollision);
        _executor.Verify(item => item.ProcessCampaignAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private CampaignEventDeliveryProcessor CreateProcessor()
    {
        var validator = new CustomerAccountRegisteredEventValidator();
        return new CampaignEventDeliveryProcessor(
            validator,
            _executor.Object,
            new CampaignEventProcessingCoordinator(_executor.Object));
    }

    private void SetupPendingPreparation(
        Guid eventId,
        PreparedCampaignTarget target)
    {
        _executor.Setup(item => item.PrepareEventAsync(
                It.IsAny<ValidatedCustomerAccountRegisteredEvent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PrepareCustomerAccountRegisteredEventResult(
                eventId,
                EventProcessingStatuses.Pending,
                Created: true,
                [target]));
    }

    private static CampaignEventDelivery CreateDelivery(bool redelivered = false)
    {
        var eventId = Guid.NewGuid();
        var envelope = new OutgoingEvent<CustomerAccountRegisteredData>(
            eventId,
            EventTypeCodes.CustomerAccountRegistered,
            EventRoutingKeys.CustomerAccountRegistered,
            OccurredAt,
            new CustomerAccountRegisteredData(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CustomerRegistrationSources.Normal,
                null));

        return new CampaignEventDelivery(
            JsonSerializer.SerializeToUtf8Bytes(
                envelope,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            eventId.ToString("D"),
            envelope.EventType,
            envelope.RoutingKey,
            redelivered);
    }

    private static ValidatedCustomerAccountRegisteredEvent Validate(
        CampaignEventDelivery delivery)
    {
        return new CustomerAccountRegisteredEventValidator().Validate(
            delivery.Body,
            delivery.MessageId,
            delivery.MessageType,
            delivery.RoutingKey);
    }

    private static PreparedCampaignTarget Target(string status)
    {
        return new PreparedCampaignTarget(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            status);
    }
}
