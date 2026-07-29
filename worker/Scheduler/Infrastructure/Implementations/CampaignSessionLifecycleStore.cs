using Campaign.Contracts.Constants;
using Scheduler.Core.Abstractions;
using Scheduler.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Persistence.Models.Context;
using CampaignModel = Persistence.Models.Campaign;

namespace Scheduler.Infrastructure.Implementations;

public sealed class CampaignSessionLifecycleStore : ICampaignSessionLifecycleStore
{
    private readonly LoyaltyHubDbContext _dbContext;

    public CampaignSessionLifecycleStore(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<CampaignSessionLifecycleCandidate>> GetDueScheduledSessionsForUpdateAsync(
        DateTime processedAt,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.CampaignSessions
            .FromSqlInterpolated($$"""
                SELECT *
                FROM campaign_sessions
                WHERE status = 'SCHEDULED'
                  AND session_start <= {{processedAt}}
                ORDER BY session_start, campaign_session_id
                LIMIT {{batchSize}}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(session => new CampaignSessionLifecycleCandidate(
                session.CampaignSessionId,
                session.CampaignId,
                session.SessionStart,
                session.SessionEnd,
                session.Status,
                session.EndedAt))
            .ToArray();
    }

    public async Task<IReadOnlyList<CampaignSessionLifecycleCandidate>> GetDueRunningSessionsForUpdateAsync(
        DateTime processedAt,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.CampaignSessions
            .FromSqlInterpolated($$"""
                SELECT *
                FROM campaign_sessions
                WHERE status = 'RUNNING'
                  AND session_end <= {{processedAt}}
                ORDER BY session_end, campaign_session_id
                LIMIT {{batchSize}}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(session => new CampaignSessionLifecycleCandidate(
                session.CampaignSessionId,
                session.CampaignId,
                session.SessionStart,
                session.SessionEnd,
                session.Status,
                session.EndedAt))
            .ToArray();
    }

    public Task ApplySessionMutationsAsync(
        IReadOnlyList<CampaignSessionLifecycleMutation> mutations,
        CancellationToken cancellationToken = default)
    {
        if (mutations.Count == 0)
        {
            return Task.CompletedTask;
        }

        var mutationMap = mutations.ToDictionary(m => m.CampaignSessionId);

        foreach (var entry in _dbContext.ChangeTracker.Entries<CampaignSession>())
        {
            if (mutationMap.TryGetValue(entry.Entity.CampaignSessionId, out var mutation))
            {
                entry.Entity.Status = mutation.TargetStatus;
                entry.Entity.EndedAt = mutation.EndedAt;
            }
        }

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Guid>> GetEligibleCampaignsForUpdateAsync(
        DateTime processedAt,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var campaigns = await _dbContext.Campaigns
            .FromSqlInterpolated($$"""
                SELECT *
                FROM campaigns AS c
                WHERE c.status = 'ACTIVE'
                  AND c.end_date <= {{processedAt}}
                  AND NOT EXISTS (
                      SELECT 1
                      FROM campaign_sessions AS s
                      WHERE s.campaign_id = c.campaign_id
                        AND s.status IN ('SCHEDULED', 'RUNNING')
                  )
                ORDER BY c.end_date, c.campaign_id
                LIMIT {{batchSize}}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        return campaigns.Select(c => c.CampaignId).ToArray();
    }

    public Task EndCampaignsAsync(
        IReadOnlyList<Guid> campaignIds,
        DateTime processedAt,
        CancellationToken cancellationToken = default)
    {
        if (campaignIds.Count == 0)
        {
            return Task.CompletedTask;
        }

        var targetIds = new HashSet<Guid>(campaignIds);

        foreach (var entry in _dbContext.ChangeTracker.Entries<CampaignModel>())
        {
            if (targetIds.Contains(entry.Entity.CampaignId))
            {
                entry.Entity.Status = CampaignStatuses.Ended;
                entry.Entity.UpdatedAt = processedAt;
            }
        }

        return Task.CompletedTask;
    }
}
