using Campaign.Contracts.Constants;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Consumer.Infrastructure.Implementations;

public sealed class CampaignEventPreparationStore : ICampaignEventPreparationStore
{
    private readonly LoyaltyHubDbContext _dbContext;

    public CampaignEventPreparationStore(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<bool> TryInsertEventAsync(
        IValidatedCampaignEvent campaignEvent,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync($$"""
            INSERT INTO event_processings (
                event_id,
                event_type,
                routing_key,
                occurred_at,
                payload,
                payload_hash,
                status,
                created_at,
                completed_at,
                failed_at
            )
            VALUES (
                {{campaignEvent.EventId}},
                {{campaignEvent.EventType}},
                {{campaignEvent.RoutingKey}},
                {{campaignEvent.OccurredAt}},
                CAST({{campaignEvent.NormalizedPayload}} AS jsonb),
                {{campaignEvent.PayloadHash}},
                {{EventProcessingStatuses.Pending}},
                {{createdAt}},
                NULL,
                NULL
            )
            ON CONFLICT (event_id) DO NOTHING
            """, cancellationToken);

        return affectedRows == 1;
    }

    public async Task<EventPreparationState?> GetEventStateAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var eventState = await _dbContext.EventProcessings
            .AsNoTracking()
            .Where(processing => processing.EventId == eventId)
            .Select(processing => new
            {
                processing.EventId,
                processing.EventType,
                processing.RoutingKey,
                processing.OccurredAt,
                processing.PayloadHash,
                processing.Status
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (eventState is null)
        {
            return null;
        }

        var targets = await _dbContext.EventCampaignProcessings
            .AsNoTracking()
            .Where(processing => processing.EventId == eventId)
            .OrderBy(processing => processing.CampaignId)
            .ThenBy(processing => processing.CampaignSessionId)
            .Select(processing => new PreparedCampaignTarget(
                processing.EventCampaignProcessingId,
                processing.CampaignId,
                processing.CampaignSessionId,
                processing.Status))
            .ToArrayAsync(cancellationToken);

        return new EventPreparationState(
            eventState.EventId,
            eventState.EventType,
            eventState.RoutingKey,
            eventState.OccurredAt,
            eventState.PayloadHash,
            eventState.Status,
            targets);
    }

    public async Task<IReadOnlyList<CampaignEventCandidate>> GetCandidateTargetsAsync(
        string eventType,
        DateTime occurredAt,
        CancellationToken cancellationToken = default)
    {
        return await (
                from campaign in _dbContext.Campaigns.AsNoTracking()
                join session in _dbContext.CampaignSessions.AsNoTracking()
                    on campaign.CampaignId equals session.CampaignId
                where campaign.EventType == eventType
                      && (campaign.Status == CampaignStatuses.Active ||
                          campaign.Status == CampaignStatuses.Ended)
                      && campaign.StartDate <= occurredAt
                      && occurredAt < campaign.EndDate
                      && (session.Status == CampaignSessionStatuses.Scheduled ||
                          session.Status == CampaignSessionStatuses.Running ||
                          session.Status == CampaignSessionStatuses.Ended)
                      && session.SessionStart <= occurredAt
                      && occurredAt < session.SessionEnd
                orderby campaign.CampaignId, session.CampaignSessionId
                select new CampaignEventCandidate(
                    campaign.CampaignId,
                    session.CampaignSessionId))
            .ToArrayAsync(cancellationToken);
    }

    public void AddTargets(
        Guid eventId,
        Guid eventCustomerId,
        IReadOnlyList<PreparedCampaignTarget> targets,
        DateTime createdAt)
    {
        foreach (var target in targets)
        {
            _dbContext.EventCampaignProcessings.Add(
                new EventCampaignProcessing
                {
                    EventCampaignProcessingId = target.EventCampaignProcessingId,
                    EventId = eventId,
                    CampaignId = target.CampaignId,
                    CampaignSessionId = target.CampaignSessionId,
                    EventCustomerId = eventCustomerId,
                    Status = target.Status,
                    AttemptCount = 0,
                    OutcomeCode = null,
                    LastError = null,
                    CreatedAt = createdAt,
                    ProcessedAt = null
                });
        }
    }

    public async Task MarkEventCompletedAsync(
        Guid eventId,
        DateTime completedAt,
        CancellationToken cancellationToken = default)
    {
        var eventProcessing = await _dbContext.EventProcessings
            .SingleAsync(
                processing => processing.EventId == eventId,
                cancellationToken);

        eventProcessing.Status = EventProcessingStatuses.Completed;
        eventProcessing.CompletedAt = completedAt;
        eventProcessing.FailedAt = null;
    }
}
