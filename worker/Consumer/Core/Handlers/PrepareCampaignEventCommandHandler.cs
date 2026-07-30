using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Requests;
using MediatR;

namespace Consumer.Core.Handlers;

public sealed class PrepareCampaignEventCommandHandler
    : IRequestHandler<PrepareCampaignEventCommand, PrepareCampaignEventResult>
{
    private readonly ICampaignEventPreparationStore _store;
    private readonly TimeProvider _timeProvider;

    public PrepareCampaignEventCommandHandler(
        ICampaignEventPreparationStore store,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<PrepareCampaignEventResult> Handle(
        PrepareCampaignEventCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.CampaignEvent);

        if (request.CampaignEvent.OccurredAt.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Campaign event OccurredAt must be a UTC timestamp.",
                nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var operationTime = _timeProvider.GetUtcNow().UtcDateTime;
        var created = await _store.TryInsertEventAsync(
            request.CampaignEvent,
            operationTime,
            cancellationToken);

        var state = await _store.GetEventStateAsync(
            request.CampaignEvent.EventId,
            cancellationToken)
            ?? throw new CampaignProcessingException(
                CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
                retriable: true);

        if (!created)
        {
            EnsureSameEvent(state, request.CampaignEvent);

            return new PrepareCampaignEventResult(
                state.EventId,
                state.Status,
                Created: false,
                state.Targets);
        }

        var candidates = await _store.GetCandidateTargetsAsync(
            request.CampaignEvent.EventTypeVersionId,
            request.CampaignEvent.OccurredAt,
            cancellationToken);

        if (candidates
            .GroupBy(candidate => candidate.CampaignId)
            .Any(group => group.Skip(1).Any()))
        {
            throw new CampaignConfigurationException(
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid);
        }

        var targets = candidates
            .OrderBy(candidate => candidate.CampaignId)
            .ThenBy(candidate => candidate.CampaignSessionId)
            .Select(candidate => new PreparedCampaignTarget(
                Guid.NewGuid(),
                candidate.CampaignId,
                candidate.CampaignSessionId,
                EventCampaignProcessingStatuses.Pending))
            .ToArray();

        if (targets.Length == 0)
        {
            await _store.MarkEventCompletedAsync(
                request.CampaignEvent.EventId,
                operationTime,
                cancellationToken);

            return new PrepareCampaignEventResult(
                request.CampaignEvent.EventId,
                EventProcessingStatuses.Completed,
                Created: true,
                targets);
        }

        _store.AddTargets(
            request.CampaignEvent.EventId,
            targets,
            operationTime);

        return new PrepareCampaignEventResult(
            request.CampaignEvent.EventId,
            EventProcessingStatuses.Pending,
            Created: true,
            targets);
    }

    private static void EnsureSameEvent(
        EventPreparationState state,
        IValidatedCampaignEvent campaignEvent)
    {
        if (state.EventType != campaignEvent.EventType ||
            state.EventTypeVersionId != campaignEvent.EventTypeVersionId ||
            state.EventVersion != campaignEvent.EventVersion ||
            state.RoutingKey != campaignEvent.RoutingKey ||
            state.OccurredAt != campaignEvent.OccurredAt ||
            state.PayloadHash != campaignEvent.PayloadHash)
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventIdCollision);
        }
    }
}
