using Campaign.Contracts.Constants;
using Campaign.Contracts.Schedules;
using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class ActivateCampaignCommandHandler
    : IRequestHandler<ActivateCampaignCommand, CampaignDetailResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICampaignConfigurationService _configurationService;
    private readonly TimeProvider _timeProvider;

    public ActivateCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        IAuditLogWriter auditLogWriter,
        ICampaignConfigurationService configurationService,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _auditLogWriter = auditLogWriter;
        _configurationService = configurationService;
        _timeProvider = timeProvider;
    }

    public async Task<CampaignDetailResult> Handle(
        ActivateCampaignCommand request,
        CancellationToken ct)
    {
        var campaign = await _campaignRepository.GetForUpdateAsync(request.CampaignId, ct);
        if (campaign is null)
        {
            throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);
        }

        campaign.EnsureCanActivate();

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var startDateUtc = DateTime.SpecifyKind(campaign.StartDate, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(campaign.EndDate, DateTimeKind.Utc);

        if (endDateUtc <= startDateUtc)
        {
            throw new DomainException("CAMPAIGN_DATE_RANGE_INVALID", DomainErrorType.Validation);
        }

        if (endDateUtc <= now)
        {
            throw new DomainException("CAMPAIGN_SCHEDULE_EMPTY", DomainErrorType.Validation);
        }

        if (campaign.DurationHour <= 0)
        {
            throw new DomainException("CAMPAIGN_DURATION_INVALID", DomainErrorType.Validation);
        }

        if (campaign.UserLimitTotal is < 0 ||
            campaign.UserLimitSession is < 0 ||
            (campaign.UserLimitTotal.HasValue &&
             campaign.UserLimitSession.HasValue &&
             campaign.UserLimitSession > campaign.UserLimitTotal))
        {
            throw new DomainException("CAMPAIGN_LIMIT_INVALID", DomainErrorType.Validation);
        }

        _configurationService.ParseCondition(campaign.EventType, campaign.Condition);

        var actions = await _campaignRepository.GetActionsForUpdateAsync(request.CampaignId, ct);
        if (actions.Count == 0)
        {
            throw new DomainException("CAMPAIGN_ACTIONS_REQUIRED", DomainErrorType.Validation);
        }

        var seenOrders = new HashSet<int>();
        foreach (var action in actions)
        {
            if (action.ExecuteOrder <= 0 || !seenOrders.Add(action.ExecuteOrder))
            {
                throw new DomainException("CAMPAIGN_ACTION_ORDER_CONFLICT", DomainErrorType.Validation);
            }

            if (action.TotalCount is < 0 ||
                action.SessionCount is < 0 ||
                (action.TotalCount.HasValue &&
                 action.SessionCount.HasValue &&
                 action.SessionCount > action.TotalCount))
            {
                throw new DomainException("CAMPAIGN_LIMIT_INVALID", DomainErrorType.Validation);
            }

            if (action.ActionType != ActionTypes.IssuePoint)
            {
                throw new DomainException("CAMPAIGN_ACTION_TYPE_INVALID", DomainErrorType.Validation);
            }

            _configurationService.ParseAction(
                campaign.EventType,
                action.ActionType,
                action.ActionConfig);
        }

        if (!CampaignScheduleCron.TryParse(campaign.ScheduleCron, out var scheduleCron) || scheduleCron is null)
        {
            throw new DomainException("CAMPAIGN_SCHEDULE_INVALID", DomainErrorType.Validation);
        }

        var occurrenceWindowStartUtc = startDateUtc > now
            ? startDateUtc
            : now;

        if (!scheduleCron.TryGetOccurrences(
                occurrenceWindowStartUtc,
                endDateUtc,
                campaign.DurationHour,
                out var occurrences,
                out var errorCode))
        {
            throw new DomainException(errorCode!, DomainErrorType.Validation);
        }

        var sessions = occurrences
            .Select(occ => CampaignSession.Create(
                campaign.CampaignId,
                occ.SessionStartUtc,
                occ.SessionEndUtc,
                now))
            .ToArray();

        _campaignRepository.AddSessions(sessions);

        campaign.Activate(now);
        await _campaignRepository.UpdateAsync(campaign, ct);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Activate,
            AuditEntityTypes.Campaign,
            campaign.CampaignId,
            null,
            CampaignAuditSerializer.Activation(campaign, scheduleCron.CanonicalCron, sessions.Length),
            null));

        var sessionResults = sessions
            .Take(100)
            .Select(s => s.ToResult())
            .ToArray();

        return campaign.ToDetailResult() with
        {
            Actions = actions.Select(a => a.ToResult()).ToArray(),
            Sessions = sessionResults,
            SessionCount = sessions.Length
        };
    }
}
