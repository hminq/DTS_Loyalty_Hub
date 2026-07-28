using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class EventProcessingFinalizationStore
    : IEventProcessingFinalizationStore
{
    private readonly LoyaltyHubDbContext _dbContext;

    public EventProcessingFinalizationStore(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<CampaignProcessingFailureState?> LockCampaignProcessingAsync(
        Guid eventCampaignProcessingId,
        CancellationToken cancellationToken = default)
    {
        var processing = await _dbContext.EventCampaignProcessings
            .FromSqlInterpolated($$"""
                SELECT *
                FROM event_campaign_processings
                WHERE event_campaign_processing_id = {{eventCampaignProcessingId}}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);

        return processing is null
            ? null
            : new CampaignProcessingFailureState(
                processing.EventCampaignProcessingId,
                processing.Status,
                processing.AttemptCount,
                processing.OutcomeCode);
    }

    public void MarkCampaignFailed(
        Guid eventCampaignProcessingId,
        string outcomeCode,
        string errorSummary,
        DateTime processedAt)
    {
        var processing = GetLockedCampaignProcessing(eventCampaignProcessingId);
        processing.Status = EventCampaignProcessingStatuses.Failed;
        processing.AttemptCount++;
        processing.OutcomeCode = outcomeCode;
        processing.LastError = errorSummary;
        processing.ProcessedAt = processedAt;
    }

    public async Task<EventProcessingFinalizationState?> LockEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var eventProcessing = await _dbContext.EventProcessings
            .FromSqlInterpolated($$"""
                SELECT *
                FROM event_processings
                WHERE event_id = {{eventId}}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);

        if (eventProcessing is null)
        {
            return null;
        }

        var children = await _dbContext.EventCampaignProcessings
            .FromSqlInterpolated($$"""
                SELECT *
                FROM event_campaign_processings
                WHERE event_id = {{eventId}}
                ORDER BY event_campaign_processing_id
                FOR UPDATE
                """)
            .ToArrayAsync(cancellationToken);

        return new EventProcessingFinalizationState(
            eventProcessing.EventId,
            eventProcessing.Status,
            children.Select(processing => processing.Status).ToArray());
    }

    public void MarkEventCompleted(Guid eventId, DateTime completedAt)
    {
        var eventProcessing = GetLockedEvent(eventId);
        eventProcessing.Status = EventProcessingStatuses.Completed;
        eventProcessing.CompletedAt = completedAt;
        eventProcessing.FailedAt = null;
    }

    public void MarkEventFailed(Guid eventId, DateTime failedAt)
    {
        var eventProcessing = GetLockedEvent(eventId);
        eventProcessing.Status = EventProcessingStatuses.Failed;
        eventProcessing.CompletedAt = null;
        eventProcessing.FailedAt = failedAt;
    }

    private EventCampaignProcessing GetLockedCampaignProcessing(
        Guid eventCampaignProcessingId)
    {
        return _dbContext.EventCampaignProcessings.Local.SingleOrDefault(
            processing =>
                processing.EventCampaignProcessingId == eventCampaignProcessingId)
            ?? throw PersistenceFailure();
    }

    private EventProcessing GetLockedEvent(Guid eventId)
    {
        return _dbContext.EventProcessings.Local.SingleOrDefault(
            processing => processing.EventId == eventId)
            ?? throw PersistenceFailure();
    }

    private static CampaignProcessingException PersistenceFailure()
    {
        return new CampaignProcessingException(
            CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
            retriable: true);
    }
}
