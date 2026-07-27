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
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public CreateCampaignActionCommandHandler(
        ICampaignRepository campaignRepository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public async Task<CampaignActionResult> Handle(
        CreateCampaignActionCommand request,
        CancellationToken ct)
    {
        var campaign = await GetDraftCampaignAsync(request.CampaignId, ct);
        var (actionType, actionConfig) = CampaignConfigurationParser.ParseAction(
            campaign.EventType,
            request.ActionType,
            request.ActionConfigJson);
        CampaignConfigurationParser.EnsureActionCompatibleWithCondition(
            campaign.EventType,
            campaign.Condition,
            actionType,
            actionConfig);

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

        var actionKey = CampaignConfigurationParser.GetActionUniquenessKey(
            campaign.EventType,
            actionType,
            actionConfig);
        var existingActions = await _campaignRepository.GetActionsForUpdateAsync(
            request.CampaignId,
            ct);
        if (existingActions.Any(existingAction =>
                CampaignConfigurationParser.GetActionUniquenessKey(
                    campaign.EventType,
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
            request.TotalAmount,
            request.SessionAmount,
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
}
