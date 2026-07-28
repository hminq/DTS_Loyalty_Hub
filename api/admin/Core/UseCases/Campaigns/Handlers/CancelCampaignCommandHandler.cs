using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class CancelCampaignCommandHandler
    : IRequestHandler<CancelCampaignCommand, CancelCampaignResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public CancelCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public async Task<CancelCampaignResult> Handle(
        CancelCampaignCommand request,
        CancellationToken ct)
    {
        if (request.CampaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        var campaign = await _campaignRepository.GetForUpdateAsync(request.CampaignId, ct)
            ?? throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);
        campaign.EnsureCanCancel();

        var cancelledAt = _timeProvider.GetUtcNow().UtcDateTime;
        var sessions = await _campaignRepository.GetOpenSessionsForUpdateAsync(
            request.CampaignId,
            ct);
        var cancelledScheduledSessionCount = sessions.Count(
            session => session.Status == CampaignSessionStatuses.Scheduled);
        var cancelledRunningSessionCount = sessions.Count(
            session => session.Status == CampaignSessionStatuses.Running);
        var oldValue = CampaignAuditSerializer.Campaign(campaign);

        foreach (var session in sessions)
        {
            session.Cancel(cancelledAt);
        }

        campaign.Cancel(cancelledAt);
        await _campaignRepository.UpdateAsync(campaign, ct);
        _campaignRepository.UpdateSessions(sessions);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Cancel,
            AuditEntityTypes.Campaign,
            campaign.CampaignId,
            oldValue,
            CampaignAuditSerializer.Cancellation(
                campaign,
                cancelledScheduledSessionCount,
                cancelledRunningSessionCount,
                cancelledAt),
            null));

        return new CancelCampaignResult(
            campaign.CampaignId,
            campaign.Status,
            cancelledScheduledSessionCount,
            cancelledRunningSessionCount,
            cancelledAt);
    }
}
