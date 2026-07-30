using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class DeleteCampaignCommandHandler : IRequestHandler<DeleteCampaignCommand>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuditLogWriter _auditLogWriter;

    public DeleteCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        IAuditLogWriter auditLogWriter)
    {
        _campaignRepository = campaignRepository;
        _auditLogWriter = auditLogWriter;
    }

    public async Task Handle(DeleteCampaignCommand request, CancellationToken ct)
    {
        if (request.CampaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        var campaign = await _campaignRepository.GetForUpdateAsync(request.CampaignId, ct)
            ?? throw new DomainException("CAMPAIGN_NOT_FOUND", DomainErrorType.NotFound);
        campaign.EnsureDraft();

        await _campaignRepository.DeleteDraftAsync(request.CampaignId, ct);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Delete,
            AuditEntityTypes.Campaign,
            request.CampaignId,
            CampaignAuditSerializer.Campaign(campaign),
            null,
            null));
    }
}
