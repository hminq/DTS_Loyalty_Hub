using Core.UseCases.Campaigns.Results;
using Core.UseCases.Common;
using DomainCampaign = Core.Entities.Campaign;
using DomainCampaignAction = Core.Entities.CampaignAction;
using DomainCampaignSession = Core.Entities.CampaignSession;

namespace Core.Abstractions;

public interface ICampaignRepository
{
    Task<PagedResult<CampaignListItemResult>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword,
        string? status,
        Guid? eventTypeId,
        CancellationToken ct = default);

    Task<CampaignDetailResult?> GetByIdAsync(
        Guid campaignId,
        int sessionLimit,
        CancellationToken ct = default);

    Task<DomainCampaign?> GetForUpdateAsync(
        Guid campaignId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<DomainCampaignSession>> GetOpenSessionsForUpdateAsync(
        Guid campaignId,
        CancellationToken ct = default);

    Task<DomainCampaignAction?> GetActionForUpdateAsync(
        Guid campaignId,
        Guid actionId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<DomainCampaignAction>> GetActionsForUpdateAsync(
        Guid campaignId,
        CancellationToken ct = default);

    Task<CampaignActionResult?> GetActionByIdAsync(
        Guid campaignId,
        Guid actionId,
        CancellationToken ct = default);

    Task<bool> ActionOrderExistsAsync(
        Guid campaignId,
        int executeOrder,
        Guid? excludedActionId,
        CancellationToken ct = default);

    DomainCampaign Add(DomainCampaign campaign);

    Task UpdateAsync(DomainCampaign campaign, CancellationToken ct = default);

    void UpdateSessions(IEnumerable<DomainCampaignSession> sessions);

    Task DeleteDraftAsync(Guid campaignId, CancellationToken ct = default);

    DomainCampaignAction AddAction(DomainCampaignAction action);

    void AddSessions(IEnumerable<DomainCampaignSession> sessions);

    Task TouchAsync(Guid campaignId, DateTime updatedAt, CancellationToken ct = default);

    Task UpdateActionAsync(DomainCampaignAction action, DateTime updatedAt, CancellationToken ct = default);

    Task DeleteActionAsync(
        Guid campaignId,
        Guid actionId,
        DateTime updatedAt,
        CancellationToken ct = default);
}
