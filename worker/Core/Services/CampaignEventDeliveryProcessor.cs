using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;

namespace Core.Services;

public sealed class CampaignEventDeliveryProcessor
{
    private readonly ICustomerAccountRegisteredEventValidator _eventValidator;
    private readonly ICampaignProcessingScopeExecutor _scopeExecutor;
    private readonly CampaignEventProcessingCoordinator _coordinator;

    public CampaignEventDeliveryProcessor(
        ICustomerAccountRegisteredEventValidator eventValidator,
        ICampaignProcessingScopeExecutor scopeExecutor,
        CampaignEventProcessingCoordinator coordinator)
    {
        _eventValidator = eventValidator ??
            throw new ArgumentNullException(nameof(eventValidator));
        _scopeExecutor = scopeExecutor ??
            throw new ArgumentNullException(nameof(scopeExecutor));
        _coordinator = coordinator ??
            throw new ArgumentNullException(nameof(coordinator));
    }

    public async Task<CampaignEventDeliveryResult> ProcessAsync(
        CampaignEventDelivery delivery,
        CancellationToken cancellationToken)
    {
        ValidatedCustomerAccountRegisteredEvent campaignEvent;
        try
        {
            campaignEvent = _eventValidator.Validate(
                delivery.Body,
                delivery.MessageId,
                delivery.MessageType,
                delivery.RoutingKey);
        }
        catch (CampaignEventValidationException exception)
        {
            return Rejected(null, exception.ErrorCode);
        }

        PrepareCustomerAccountRegisteredEventResult preparation;
        try
        {
            preparation = await _scopeExecutor.PrepareEventAsync(
                campaignEvent,
                cancellationToken);
        }
        catch (CampaignEventValidationException exception)
        {
            return Rejected(campaignEvent.EventId, exception.ErrorCode);
        }

        if (preparation.Status == EventProcessingStatuses.Completed)
        {
            return Acknowledged(
                campaignEvent.EventId,
                preparation.Targets.Count,
                completedCampaignCount: preparation.Targets.Count(target =>
                    target.Status == EventCampaignProcessingStatuses.Completed),
                skippedCampaignCount: preparation.Targets.Count(target =>
                    target.Status == EventCampaignProcessingStatuses.Skipped));
        }

        if (preparation.Status == EventProcessingStatuses.Failed)
        {
            return Rejected(campaignEvent.EventId, null);
        }

        var sequence = await _coordinator.ProcessAsync(
            campaignEvent.EventId,
            preparation.Targets,
            cancellationToken);

        return sequence.Finalization.Status switch
        {
            EventProcessingStatuses.Completed => Acknowledged(
                campaignEvent.EventId,
                preparation.Targets.Count,
                sequence.Finalization.CompletedCampaignCount,
                sequence.Finalization.SkippedCampaignCount),
            EventProcessingStatuses.Failed => new CampaignEventDeliveryResult(
                CampaignEventDeliveryDispositions.Reject,
                campaignEvent.EventId,
                null,
                preparation.Targets.Count,
                sequence.Finalization.CompletedCampaignCount,
                sequence.Finalization.SkippedCampaignCount,
                sequence.Finalization.FailedCampaignCount),
            _ => throw new CampaignProcessingException(
                CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
                retriable: true)
        };
    }

    private static CampaignEventDeliveryResult Acknowledged(
        Guid eventId,
        int pinnedCampaignCount,
        int completedCampaignCount,
        int skippedCampaignCount)
    {
        return new CampaignEventDeliveryResult(
            CampaignEventDeliveryDispositions.Acknowledge,
            eventId,
            null,
            pinnedCampaignCount,
            completedCampaignCount,
            skippedCampaignCount,
            0);
    }

    private static CampaignEventDeliveryResult Rejected(
        Guid? eventId,
        string? errorCode)
    {
        return new CampaignEventDeliveryResult(
            CampaignEventDeliveryDispositions.Reject,
            eventId,
            errorCode,
            0,
            0,
            0,
            0);
    }
}
