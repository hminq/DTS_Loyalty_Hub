using Core.Abstractions;
using Core.Entities.Constants;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Results;
using MediatR;
using DomainCampaign = Core.Entities.Campaign;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class CreateCampaignCommandHandler
    : IRequestHandler<CreateCampaignCommand, CampaignDetailResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public CreateCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public Task<CampaignDetailResult> Handle(
        CreateCampaignCommand request,
        CancellationToken ct)
    {
        var (eventType, condition) = CampaignConfigurationParser.ParseCondition(
            request.EventType,
            request.ConditionJson);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var campaign = DomainCampaign.Create(
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
            now);

        _campaignRepository.Add(campaign);
        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Create,
            AuditEntityTypes.Campaign,
            campaign.CampaignId,
            null,
            CampaignAuditSerializer.Campaign(campaign),
            null));

        return Task.FromResult(campaign.ToDetailResult());
    }
}
