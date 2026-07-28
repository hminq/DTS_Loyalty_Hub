using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class UpdateCampaignCommandHandler
    : IRequestHandler<UpdateCampaignCommand, CampaignDetailResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICampaignConfigurationService _configurationService;
    private readonly TimeProvider _timeProvider;

    public UpdateCampaignCommandHandler(
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
        UpdateCampaignCommand request,
        CancellationToken ct)
    {
        if (request.CampaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        var campaign = await _campaignRepository.GetForUpdateAsync(request.CampaignId, ct)
            ?? throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);
        var existingDetail = await _campaignRepository.GetByIdAsync(request.CampaignId, 100, ct)
            ?? throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);
        var oldValue = CampaignAuditSerializer.Campaign(campaign);
        var (eventType, condition) = _configurationService.ParseCondition(
            request.EventType,
            request.ConditionJson);

        foreach (var action in existingDetail.Actions)
        {
            _configurationService.EnsureActionCompatibleWithCondition(
                eventType,
                condition,
                action.ActionType,
                action.ActionConfig);
        }

        campaign.Update(
            request.CampaignName,
            request.Description,
            request.BannerImageUrl,
            eventType,
            condition,
            request.StartDate.UtcDateTime,
            request.EndDate.UtcDateTime,
            request.ScheduleCron,
            request.DurationHour,
            request.UserLimitTotal,
            request.UserLimitSession,
            _timeProvider.GetUtcNow().UtcDateTime);

        await _campaignRepository.UpdateAsync(campaign, ct);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Update,
            AuditEntityTypes.Campaign,
            campaign.CampaignId,
            oldValue,
            CampaignAuditSerializer.Campaign(campaign),
            null));

        return campaign.ToDetailResult(existingDetail);
    }
}
