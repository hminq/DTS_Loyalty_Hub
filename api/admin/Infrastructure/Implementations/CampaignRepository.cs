using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.UseCases.Campaigns.Results;
using Core.UseCases.Common;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;
using DomainCampaign = Core.Entities.Campaign;
using DomainCampaignAction = Core.Entities.CampaignAction;
using DomainCampaignSession = Core.Entities.CampaignSession;
using PersistenceAction = Persistence.Models.Action;
using PersistenceCampaign = Persistence.Models.Campaign;
using PersistenceCampaignSession = Persistence.Models.CampaignSession;

namespace Infrastructure.Implementations;

public sealed class CampaignRepository : ICampaignRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public CampaignRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CampaignListItemResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? status,
        Guid? eventTypeId,
        CancellationToken ct = default)
    {
        IQueryable<PersistenceCampaign> query = _dbContext.Campaigns
            .AsNoTracking()
            .Include(campaign => campaign.EventTypeVersion)
            .ThenInclude(version => version.EventType);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(campaign =>
                EF.Functions.ILike(campaign.CampaignName, pattern));
        }

        if (status is not null)
        {
            query = query.Where(campaign => campaign.Status == status);
        }

        if (eventTypeId is not null)
        {
            query = query.Where(campaign => campaign.EventTypeVersion.EventTypeId == eventTypeId.Value);
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(campaign => campaign.CreatedAt)
            .ThenBy(campaign => campaign.CampaignName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(campaign => new CampaignListItemResult(
                campaign.CampaignId,
                campaign.CampaignName,
                new CampaignEventDefinitionReferenceResult(
                    campaign.EventTypeVersion.EventType.EventTypeId,
                    campaign.EventTypeVersion.EventTypeVersionId,
                    campaign.EventTypeVersion.EventType.Code,
                    campaign.EventTypeVersion.EventType.Name,
                    campaign.EventTypeVersion.Version),
                campaign.Status,
                campaign.StartDate,
                campaign.EndDate,
                campaign.ScheduleCron,
                campaign.DurationHour,
                _dbContext.Actions.Count(action =>
                    action.ReferenceType == ActionReferenceTypes.Campaign &&
                    action.ReferenceId == campaign.CampaignId),
                campaign.CampaignSessions
                    .Where(session => session.Status == CampaignSessionStatuses.Scheduled)
                    .Select(session => (DateTime?)session.SessionStart)
                    .Min(),
                campaign.CreatedAt,
                campaign.UpdatedAt))
            .ToArrayAsync(ct);

        return new PagedResult<CampaignListItemResult>(
            items,
            page,
            pageSize,
            totalItems);
    }

    public async Task<CampaignDetailResult?> GetByIdAsync(
        Guid campaignId,
        int sessionLimit,
        CancellationToken ct = default)
    {
        var campaign = await _dbContext.Campaigns
            .AsNoTracking()
            .Include(item => item.EventTypeVersion)
            .ThenInclude(version => version.EventType)
            .Where(item => item.CampaignId == campaignId)
            .Select(item => new
            {
                item.CampaignId,
                item.CampaignName,
                item.Description,
                item.BannerImageUrl,
                item.EventTypeVersion,
                item.StartDate,
                item.EndDate,
                item.Condition,
                item.ScheduleCron,
                item.DurationHour,
                item.UserLimitTotal,
                item.UserLimitSession,
                item.Status,
                item.CreatedAt,
                item.UpdatedAt
            })
            .SingleOrDefaultAsync(ct);

        if (campaign is null)
        {
            return null;
        }

        var actions = await _dbContext.Actions
            .AsNoTracking()
            .Where(action =>
                action.ReferenceType == ActionReferenceTypes.Campaign &&
                action.ReferenceId == campaignId)
            .OrderBy(action => action.ExecuteOrder)
            .ThenBy(action => action.ActionId)
            .Select(action => new CampaignActionResult(
                action.ActionId,
                action.ActionType,
                action.ActionConfig,
                action.ExecuteOrder,
                action.TotalCount,
                action.SessionCount,
                action.UsedCount,
                action.CreatedAt))
            .ToArrayAsync(ct);

        var sessionCount = await _dbContext.CampaignSessions
            .AsNoTracking()
            .CountAsync(session => session.CampaignId == campaignId, ct);

        var sessions = await _dbContext.CampaignSessions
            .AsNoTracking()
            .Where(session => session.CampaignId == campaignId)
            .OrderBy(session => session.SessionStart)
            .ThenBy(session => session.CampaignSessionId)
            .Take(sessionLimit)
            .Select(session => new CampaignSessionResult(
                session.CampaignSessionId,
                session.SessionStart,
                session.SessionEnd,
                session.Status,
                session.CreatedAt,
                session.EndedAt))
            .ToArrayAsync(ct);

        return new CampaignDetailResult(
            campaign.CampaignId,
            campaign.CampaignName,
            campaign.Description,
            campaign.BannerImageUrl,
            null,
            new CampaignEventDefinitionReferenceResult(
                campaign.EventTypeVersion.EventType.EventTypeId,
                campaign.EventTypeVersion.EventTypeVersionId,
                campaign.EventTypeVersion.EventType.Code,
                campaign.EventTypeVersion.EventType.Name,
                campaign.EventTypeVersion.Version),
            campaign.StartDate,
            campaign.EndDate,
            campaign.Condition,
            campaign.ScheduleCron,
            campaign.DurationHour,
            campaign.UserLimitTotal,
            campaign.UserLimitSession,
            campaign.Status,
            campaign.CreatedAt,
            campaign.UpdatedAt,
            actions,
            sessions,
            sessionCount);
    }

    public async Task<DomainCampaign?> GetForUpdateAsync(
        Guid campaignId,
        CancellationToken ct = default)
    {
        var campaign = await _dbContext.Campaigns
            .FromSqlInterpolated($"SELECT * FROM campaigns WHERE campaign_id = {campaignId} FOR UPDATE")
            .AsNoTracking()
            .SingleOrDefaultAsync(ct);

        return campaign is null
            ? null
            : DomainCampaign.Restore(
                campaign.CampaignId,
                campaign.CampaignName,
                campaign.Description,
                campaign.BannerImageUrl,
                campaign.EventTypeVersionId,
                campaign.Condition,
                campaign.StartDate,
                campaign.EndDate,
                campaign.ScheduleCron ?? string.Empty,
                campaign.DurationHour ?? 0,
                campaign.UserLimitTotal,
                campaign.UserLimitSession,
                campaign.Status,
                campaign.CreatedAt,
                campaign.UpdatedAt);
    }

    public async Task<IReadOnlyCollection<DomainCampaignSession>> GetOpenSessionsForUpdateAsync(
        Guid campaignId,
        CancellationToken ct = default)
    {
        var sessions = await _dbContext.CampaignSessions
            .FromSqlInterpolated($$"""
                SELECT *
                FROM campaign_sessions
                WHERE campaign_id = {{campaignId}}
                  AND status IN (
                      {{CampaignSessionStatuses.Scheduled}},
                      {{CampaignSessionStatuses.Running}}
                  )
                ORDER BY session_start, campaign_session_id
                FOR UPDATE
                """)
            .ToArrayAsync(ct);

        return sessions.Select(ToDomainSession).ToArray();
    }

    public async Task<DomainCampaignAction?> GetActionForUpdateAsync(
        Guid campaignId,
        Guid actionId,
        CancellationToken ct = default)
    {
        var action = await _dbContext.Actions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.ActionId == actionId &&
                    item.ReferenceType == ActionReferenceTypes.Campaign &&
                    item.ReferenceId == campaignId,
                ct);

        return action is null ? null : ToDomainAction(action);
    }

    public async Task<IReadOnlyCollection<DomainCampaignAction>> GetActionsForUpdateAsync(
        Guid campaignId,
        CancellationToken ct = default)
    {
        var actions = await _dbContext.Actions
            .AsNoTracking()
            .Where(action =>
                action.ReferenceType == ActionReferenceTypes.Campaign &&
                action.ReferenceId == campaignId)
            .OrderBy(action => action.ExecuteOrder)
            .ThenBy(action => action.ActionId)
            .ToArrayAsync(ct);

        return actions.Select(ToDomainAction).ToArray();
    }

    public Task<CampaignActionResult?> GetActionByIdAsync(
        Guid campaignId,
        Guid actionId,
        CancellationToken ct = default)
    {
        return _dbContext.Actions
            .AsNoTracking()
            .Where(action =>
                action.ActionId == actionId &&
                action.ReferenceType == ActionReferenceTypes.Campaign &&
                action.ReferenceId == campaignId)
            .Select(action => new CampaignActionResult(
                action.ActionId,
                action.ActionType,
                action.ActionConfig,
                action.ExecuteOrder,
                action.TotalCount,
                action.SessionCount,
                action.UsedCount,
                action.CreatedAt))
            .SingleOrDefaultAsync(ct);
    }

    public Task<bool> ActionOrderExistsAsync(
        Guid campaignId,
        int executeOrder,
        Guid? excludedActionId,
        CancellationToken ct = default)
    {
        return _dbContext.Actions
            .AsNoTracking()
            .AnyAsync(
                action =>
                    action.ReferenceType == ActionReferenceTypes.Campaign &&
                    action.ReferenceId == campaignId &&
                    action.ExecuteOrder == executeOrder &&
                    (!excludedActionId.HasValue || action.ActionId != excludedActionId.Value),
                ct);
    }

    public DomainCampaign Add(DomainCampaign campaign)
    {
        _dbContext.Campaigns.Add(new PersistenceCampaign
        {
            CampaignId = campaign.CampaignId,
            CampaignName = campaign.CampaignName,
            Description = campaign.Description,
            BannerImageUrl = campaign.BannerImageUrl,
            EventTypeVersionId = campaign.EventTypeVersionId,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Condition = campaign.Condition,
            ScheduleCron = campaign.ScheduleCron,
            DurationHour = campaign.DurationHour,
            UserLimitTotal = campaign.UserLimitTotal,
            UserLimitSession = campaign.UserLimitSession,
            Status = campaign.Status,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        });

        return campaign;
    }

    public async Task UpdateAsync(
        DomainCampaign campaign,
        CancellationToken ct = default)
    {
        var persistedCampaign = await GetPersistedCampaignAsync(campaign.CampaignId, ct);
        persistedCampaign.CampaignName = campaign.CampaignName;
        persistedCampaign.Description = campaign.Description;
        persistedCampaign.BannerImageUrl = campaign.BannerImageUrl;
        persistedCampaign.EventTypeVersionId = campaign.EventTypeVersionId;
        persistedCampaign.StartDate = campaign.StartDate;
        persistedCampaign.EndDate = campaign.EndDate;
        persistedCampaign.Condition = campaign.Condition;
        persistedCampaign.ScheduleCron = campaign.ScheduleCron;
        persistedCampaign.DurationHour = campaign.DurationHour;
        persistedCampaign.UserLimitTotal = campaign.UserLimitTotal;
        persistedCampaign.UserLimitSession = campaign.UserLimitSession;
        persistedCampaign.Status = campaign.Status;
        persistedCampaign.UpdatedAt = campaign.UpdatedAt;
    }

    public void UpdateSessions(IEnumerable<DomainCampaignSession> sessions)
    {
        foreach (var session in sessions)
        {
            var persistedSession = _dbContext.CampaignSessions.Local.SingleOrDefault(
                item => item.CampaignSessionId == session.CampaignSessionId)
                ?? throw new Core.Exceptions.DomainException(
                    "CAMPAIGN_SESSION_NOT_FOUND",
                    Core.Exceptions.DomainErrorType.NotFound);

            persistedSession.Status = session.Status;
            persistedSession.EndedAt = session.EndedAt;
        }
    }

    public async Task DeleteDraftAsync(
        Guid campaignId,
        CancellationToken ct = default)
    {
        var actions = await _dbContext.Actions
            .Where(action =>
                action.ReferenceType == ActionReferenceTypes.Campaign &&
                action.ReferenceId == campaignId)
            .ToArrayAsync(ct);
        var campaign = await GetPersistedCampaignAsync(campaignId, ct);

        _dbContext.Actions.RemoveRange(actions);
        _dbContext.Campaigns.Remove(campaign);
    }

    public DomainCampaignAction AddAction(DomainCampaignAction action)
    {
        _dbContext.Actions.Add(new PersistenceAction
        {
            ActionId = action.ActionId,
            ReferenceType = ActionReferenceTypes.Campaign,
            ReferenceId = action.CampaignId,
            ActionType = action.ActionType,
            ActionConfig = action.ActionConfig,
            ExecuteOrder = action.ExecuteOrder,
            TotalCount = action.TotalCount,
            SessionCount = action.SessionCount,
            UsedCount = action.UsedCount,
            CreatedAt = action.CreatedAt
        });

        return action;
    }

    public void AddSessions(IEnumerable<DomainCampaignSession> sessions)
    {
        foreach (var session in sessions)
        {
            _dbContext.CampaignSessions.Add(new PersistenceCampaignSession
            {
                CampaignSessionId = session.CampaignSessionId,
                CampaignId = session.CampaignId,
                SessionStart = session.SessionStart,
                SessionEnd = session.SessionEnd,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                EndedAt = session.EndedAt
            });
        }
    }

    public async Task TouchAsync(
        Guid campaignId,
        DateTime updatedAt,
        CancellationToken ct = default)
    {
        var campaign = await GetPersistedCampaignAsync(campaignId, ct);
        campaign.UpdatedAt = updatedAt;
    }

    public async Task UpdateActionAsync(
        DomainCampaignAction action,
        DateTime updatedAt,
        CancellationToken ct = default)
    {
        var persistedAction = await _dbContext.Actions.SingleOrDefaultAsync(
            item =>
                item.ActionId == action.ActionId &&
                item.ReferenceType == ActionReferenceTypes.Campaign &&
                item.ReferenceId == action.CampaignId,
            ct);

        if (persistedAction is null)
        {
            throw new Core.Exceptions.DomainException(
                "CAMPAIGN_ACTION_NOT_FOUND",
                Core.Exceptions.DomainErrorType.NotFound);
        }

        persistedAction.ActionType = action.ActionType;
        persistedAction.ActionConfig = action.ActionConfig;
        persistedAction.ExecuteOrder = action.ExecuteOrder;
        persistedAction.TotalCount = action.TotalCount;
        persistedAction.SessionCount = action.SessionCount;

        await TouchAsync(action.CampaignId, updatedAt, ct);
    }

    public async Task DeleteActionAsync(
        Guid campaignId,
        Guid actionId,
        DateTime updatedAt,
        CancellationToken ct = default)
    {
        var action = await _dbContext.Actions.SingleOrDefaultAsync(
            item =>
                item.ActionId == actionId &&
                item.ReferenceType == ActionReferenceTypes.Campaign &&
                item.ReferenceId == campaignId,
            ct);

        if (action is null)
        {
            throw new Core.Exceptions.DomainException(
                "CAMPAIGN_ACTION_NOT_FOUND",
                Core.Exceptions.DomainErrorType.NotFound);
        }

        _dbContext.Actions.Remove(action);
        await TouchAsync(campaignId, updatedAt, ct);
    }

    private async Task<PersistenceCampaign> GetPersistedCampaignAsync(
        Guid campaignId,
        CancellationToken ct)
    {
        return await _dbContext.Campaigns
            .SingleOrDefaultAsync(campaign => campaign.CampaignId == campaignId, ct)
            ?? throw new Core.Exceptions.DomainException(
                "CAMPAIGN_NOT_FOUND",
                Core.Exceptions.DomainErrorType.NotFound);
    }

    private static DomainCampaignAction ToDomainAction(PersistenceAction action)
    {
        return DomainCampaignAction.Restore(
            action.ActionId,
            action.ReferenceId,
            action.ActionType,
            action.ActionConfig,
            action.ExecuteOrder,
            action.TotalCount,
            action.SessionCount,
            action.UsedCount,
            action.CreatedAt);
    }

    private static DomainCampaignSession ToDomainSession(PersistenceCampaignSession session)
    {
        return DomainCampaignSession.Restore(
            session.CampaignSessionId,
            session.CampaignId,
            session.SessionStart,
            session.SessionEnd,
            session.Status,
            session.CreatedAt,
            session.EndedAt);
    }
}
