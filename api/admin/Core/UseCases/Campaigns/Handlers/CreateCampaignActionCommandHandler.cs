using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Results;
using MediatR;
using DomainCampaign = Core.Entities.Campaign;
using DomainCampaignAction = Core.Entities.CampaignAction;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class CreateCampaignActionCommandHandler
    : IRequestHandler<CreateCampaignActionCommand, CampaignActionResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignEventDefinitionRepository _eventDefinitionRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICampaignConfigurationService _configurationService;
    private readonly TimeProvider _timeProvider;

    public CreateCampaignActionCommandHandler(
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

    public async Task<CampaignActionResult> Handle(
        CreateCampaignActionCommand request,
        CancellationToken ct)
    {
        var campaign = await GetDraftCampaignAsync(request.CampaignId, ct);
        var eventDefinition = await GetSelectableEventDefinitionAsync(campaign.EventTypeVersionId, ct);
        var (actionType, actionConfig) = _configurationService.ParseAction(
            eventDefinition,
            request.ActionType,
            request.ActionConfigJson);

        if (await _campaignRepository.ActionOrderExistsAsync(
                request.CampaignId,
                request.ExecuteOrder,
                null,
                ct))
        {
            throw new DomainException(
                "CAMPAIGN_ACTION_ORDER_CONFLICT",
                DomainErrorType.Conflict);
        }

        var actionKey = _configurationService.GetActionUniquenessKey(
            eventDefinition,
            actionType,
            actionConfig);
        var existingActions = await _campaignRepository.GetActionsForUpdateAsync(
            request.CampaignId,
            ct);
        if (existingActions.Any(existingAction =>
                _configurationService.GetActionUniquenessKey(
                    eventDefinition,
                    existingAction.ActionType,
                    existingAction.ActionConfig) == actionKey))
        {
            throw new DomainException(
                "CAMPAIGN_ACTION_DUPLICATE",
                DomainErrorType.Conflict);
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var action = DomainCampaignAction.Create(
            request.CampaignId,
            actionType,
            actionConfig,
            request.ExecuteOrder,
            request.TotalCount,
            request.SessionCount,
            now);

        _campaignRepository.AddAction(action);
        await _campaignRepository.TouchAsync(request.CampaignId, now, ct);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Create,
            AuditEntityTypes.CampaignAction,
            action.ActionId,
            null,
            CampaignAuditSerializer.Action(action),
            null));

        return action.ToResult();
    }

    private async Task<DomainCampaign> GetDraftCampaignAsync(
        Guid campaignId,
        CancellationToken ct)
    {
        if (campaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        var campaign = await _campaignRepository.GetForUpdateAsync(campaignId, ct)
            ?? throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);
        campaign.EnsureDraft();
        return campaign;
    }

    private async Task<CampaignEventDefinitionResult> GetSelectableEventDefinitionAsync(
        Guid eventTypeVersionId,
        CancellationToken ct)
    {
        return await _eventDefinitionRepository.GetForCampaignWriteAsync(eventTypeVersionId, ct)
            ?? throw new DomainException(
                "CAMPAIGN_EVENT_TYPE_VERSION_NOT_FOUND",
                DomainErrorType.Validation);
    }
}
