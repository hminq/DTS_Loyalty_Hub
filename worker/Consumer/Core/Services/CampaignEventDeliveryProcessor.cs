using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Requests;

namespace Consumer.Core.Services;

public sealed class CampaignEventDeliveryProcessor
{
    private readonly IVersionedEnvelopeParser _envelopeParser;
    private readonly IEventDefinitionProvider _definitionProvider;
    private readonly GenericCampaignEventFactory _eventFactory;
    private readonly ICampaignProcessingScopeExecutor _scopeExecutor;
    private readonly CampaignEventProcessingCoordinator _coordinator;

    public CampaignEventDeliveryProcessor(
        IVersionedEnvelopeParser envelopeParser,
        IEventDefinitionProvider definitionProvider,
        GenericCampaignEventFactory eventFactory,
        ICampaignProcessingScopeExecutor scopeExecutor,
        CampaignEventProcessingCoordinator coordinator)
    {
        _envelopeParser = envelopeParser ??
            throw new ArgumentNullException(nameof(envelopeParser));
        _definitionProvider = definitionProvider ??
            throw new ArgumentNullException(nameof(definitionProvider));
        _eventFactory = eventFactory ??
            throw new ArgumentNullException(nameof(eventFactory));
        _scopeExecutor = scopeExecutor ??
            throw new ArgumentNullException(nameof(scopeExecutor));
        _coordinator = coordinator ??
            throw new ArgumentNullException(nameof(coordinator));
    }

    public async Task<CampaignEventDeliveryResult> ProcessAsync(
        CampaignEventDelivery delivery,
        CancellationToken cancellationToken)
    {
        IValidatedCampaignEvent campaignEvent;
        try
        {
            var envelope = _envelopeParser.Parse(
                delivery.Body.Span,
                delivery.MessageId,
                delivery.MessageType);
            var definition = await _definitionProvider.GetDefinitionAsync(
                envelope.EventType,
                envelope.EventVersion,
                cancellationToken);
            if (definition is null)
            {
                return Rejected(envelope.EventId, CampaignProcessingErrorCodes.EventTypeUnsupported);
            }

            campaignEvent = _eventFactory.Create(
                envelope,
                definition,
                delivery.RoutingKey);
        }
        catch (EventEnvelopeParseException exception)
        {
            return Rejected(null, exception.ErrorCode);
        }
        catch (CampaignEventValidationException exception)
        {
            return Rejected(null, exception.ErrorCode);
        }

        PrepareCampaignEventResult preparation;
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
