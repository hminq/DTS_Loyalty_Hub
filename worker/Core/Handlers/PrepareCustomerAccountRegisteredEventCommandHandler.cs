using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class PrepareCustomerAccountRegisteredEventCommandHandler
    : IRequestHandler<
        PrepareCustomerAccountRegisteredEventCommand,
        PrepareCustomerAccountRegisteredEventResult>
{
    private readonly ICampaignEventPreparationStore _store;
    private readonly TimeProvider _timeProvider;

    public PrepareCustomerAccountRegisteredEventCommandHandler(
        ICampaignEventPreparationStore store,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<PrepareCustomerAccountRegisteredEventResult> Handle(
        PrepareCustomerAccountRegisteredEventCommand request,
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

            return new PrepareCustomerAccountRegisteredEventResult(
                state.EventId,
                state.Status,
                Created: false,
                state.Targets);
        }

        var candidates = await _store.GetCandidateTargetsAsync(
            request.CampaignEvent.EventType,
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

            return new PrepareCustomerAccountRegisteredEventResult(
                request.CampaignEvent.EventId,
                EventProcessingStatuses.Completed,
                Created: true,
                targets);
        }

        _store.AddTargets(
            request.CampaignEvent.EventId,
            request.CampaignEvent.CustomerId,
            targets,
            operationTime);

        return new PrepareCustomerAccountRegisteredEventResult(
            request.CampaignEvent.EventId,
            EventProcessingStatuses.Pending,
            Created: true,
            targets);
    }

    private static void EnsureSameEvent(
        EventPreparationState state,
        ValidatedCustomerAccountRegisteredEvent campaignEvent)
    {
        if (state.EventType != campaignEvent.EventType ||
            state.RoutingKey != campaignEvent.RoutingKey ||
            state.OccurredAt != campaignEvent.OccurredAt ||
            state.PayloadHash != campaignEvent.PayloadHash)
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventIdCollision);
        }
    }
}
