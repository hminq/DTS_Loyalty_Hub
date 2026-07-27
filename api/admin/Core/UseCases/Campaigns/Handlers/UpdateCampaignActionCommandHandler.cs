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
    private readonly TimeProvider _timeProvider;

    public UpdateCampaignActionCommandHandler(
        ICampaignRepository campaignRepository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _auditLogWriter = auditLogWriter;
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
        var (actionType, actionConfig) = CampaignConfigurationParser.ParseAction(
            campaign.EventType,
            request.ActionType,
            request.ActionConfigJson);

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

        action.Update(
            actionType,
            actionConfig,
            request.ExecuteOrder,
            request.TotalCount,
            request.SessionCount,
            request.TotalAmount,
            request.SessionAmount);

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
