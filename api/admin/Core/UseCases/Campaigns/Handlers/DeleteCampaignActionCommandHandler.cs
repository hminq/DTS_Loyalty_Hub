using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class DeleteCampaignActionCommandHandler
    : IRequestHandler<DeleteCampaignActionCommand>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public DeleteCampaignActionCommandHandler(
        ICampaignRepository campaignRepository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteCampaignActionCommand request, CancellationToken ct)
    {
        if (request.CampaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        if (request.ActionId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ACTION_ID_REQUIRED", DomainErrorType.Validation);
        }

        var campaign = await _campaignRepository.GetForUpdateAsync(request.CampaignId, ct)
            ?? throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);
        campaign.EnsureDraft();

        var action = await _campaignRepository.GetActionForUpdateAsync(
                request.CampaignId,
                request.ActionId,
                ct)
            ?? throw new DomainException("CAMPAIGN_ACTION_NOT_FOUND", DomainErrorType.NotFound);

        await _campaignRepository.DeleteActionAsync(
            request.CampaignId,
            request.ActionId,
            _timeProvider.GetUtcNow().UtcDateTime,
            ct);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Delete,
            AuditEntityTypes.CampaignAction,
            request.ActionId,
            CampaignAuditSerializer.Action(action),
            null,
            null));
    }
}
