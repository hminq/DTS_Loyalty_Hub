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
    private readonly ICampaignEventDefinitionRepository _eventDefinitionRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICampaignConfigurationService _configurationService;
    private readonly TimeProvider _timeProvider;

    public UpdateCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        ICampaignEventDefinitionRepository eventDefinitionRepository,
        IAuditLogWriter auditLogWriter,
        ICampaignConfigurationService configurationService,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _eventDefinitionRepository = eventDefinitionRepository;
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
        var eventDefinition = await GetSelectableEventDefinitionAsync(request.EventTypeVersionId, ct);
        var condition = _configurationService.ParseCondition(
            eventDefinition,
            request.ConditionJson);

        foreach (var action in existingDetail.Actions)
        {
            _configurationService.ValidateAction(
                eventDefinition,
                action.ActionType,
                action.ActionConfig);
        }

        campaign.Update(
            request.CampaignName,
            request.Description,
            request.BannerImageUrl,
            eventDefinition.EventTypeVersionId,
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

        return campaign.ToDetailResult(eventDefinition, existingDetail);
    }

    private async Task<CampaignEventDefinitionResult> GetSelectableEventDefinitionAsync(
        Guid eventTypeVersionId,
        CancellationToken ct)
    {
        if (eventTypeVersionId == Guid.Empty)
        {
            throw new DomainException(
                "CAMPAIGN_EVENT_TYPE_VERSION_REQUIRED",
                DomainErrorType.Validation);
        }

        return await _eventDefinitionRepository.GetForCampaignWriteAsync(eventTypeVersionId, ct)
            ?? throw new DomainException(
                "CAMPAIGN_EVENT_TYPE_VERSION_NOT_FOUND",
                DomainErrorType.Validation);
    }
}
