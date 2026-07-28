using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class UpdateCampaignActionCommandHandler
    : IRequestHandler<UpdateCampaignActionCommand, CampaignActionResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICampaignConfigurationService _configurationService;
    private readonly TimeProvider _timeProvider;

    public UpdateCampaignActionCommandHandler(
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

    public async Task<CampaignActionResult> Handle(
        UpdateCampaignActionCommand request,
        CancellationToken ct)
    {
        ValidateIds(request.CampaignId, request.ActionId);
        var campaign = await _campaignRepository.GetForUpdateAsync(request.CampaignId, ct)
            ?? throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);
        campaign.EnsureDraft();

        var action = await _campaignRepository.GetActionForUpdateAsync(
                request.CampaignId,
                request.ActionId,
                ct)
            ?? throw new DomainException("CAMPAIGN_ACTION_NOT_FOUND", DomainErrorType.NotFound);
        var oldValue = CampaignAuditSerializer.Action(action);
        var (actionType, actionConfig) = _configurationService.ParseAction(
            campaign.EventType,
            request.ActionType,
            request.ActionConfigJson);
        _configurationService.EnsureActionCompatibleWithCondition(
            campaign.EventType,
            campaign.Condition,
            actionType,
            actionConfig);

        if (await _campaignRepository.ActionOrderExistsAsync(
                request.CampaignId,
                request.ExecuteOrder,
                request.ActionId,
                ct))
        {
            throw new DomainException(
                "CAMPAIGN_ACTION_ORDER_CONFLICT",
                DomainErrorType.Conflict);
        }

        var actionKey = _configurationService.GetActionUniquenessKey(
            campaign.EventType,
            actionType,
            actionConfig);
        var existingActions = await _campaignRepository.GetActionsForUpdateAsync(
            request.CampaignId,
            ct);
        if (existingActions.Any(existingAction =>
                existingAction.ActionId != request.ActionId &&
                _configurationService.GetActionUniquenessKey(
                    campaign.EventType,
                    existingAction.ActionType,
                    existingAction.ActionConfig) == actionKey))
        {
            throw new DomainException(
                "CAMPAIGN_ACTION_DUPLICATE",
                DomainErrorType.Conflict);
        }

        action.Update(
            actionType,
            actionConfig,
            request.ExecuteOrder,
            request.TotalCount,
            request.SessionCount);

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        await _campaignRepository.UpdateActionAsync(action, now, ct);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Update,
            AuditEntityTypes.CampaignAction,
            action.ActionId,
            oldValue,
            CampaignAuditSerializer.Action(action),
            null));

        return action.ToResult();
    }

    private static void ValidateIds(Guid campaignId, Guid actionId)
    {
        if (campaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        if (actionId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ACTION_ID_REQUIRED", DomainErrorType.Validation);
        }
    }
}
