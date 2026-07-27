using Core.UseCases.Campaigns.Results;
using DomainCampaign = Core.Entities.Campaign;
using DomainCampaignAction = Core.Entities.CampaignAction;
using DomainCampaignSession = Core.Entities.CampaignSession;

namespace Core.UseCases.Campaigns;

internal static class CampaignResultMapper
{
    public static CampaignDetailResult ToDetailResult(
        this DomainCampaign campaign,
        CampaignDetailResult? existing = null)
    {
        return new CampaignDetailResult(
            campaign.CampaignId,
            campaign.CampaignName,
            campaign.Description,
            campaign.BannerImageUrl,
            null,
            campaign.EventType,
            campaign.StartDate,
            campaign.EndDate,
            campaign.Condition,
            campaign.ScheduleCron,
            campaign.DurationHour,
            campaign.UserLimitTotal,
            campaign.UserLimitSession,
            campaign.Status,
            campaign.CreatedAt,
            campaign.UpdatedAt,
            existing?.Actions ?? [],
            existing?.Sessions ?? [],
            existing?.SessionCount ?? 0);
    }

    public static CampaignActionResult ToResult(this DomainCampaignAction action)
    {
        return new CampaignActionResult(
            action.ActionId,
            action.ActionType,
            action.ActionConfig,
            action.ExecuteOrder,
            action.TotalCount,
            action.SessionCount,
            action.UsedCount,
            action.CreatedAt);
    }

    public static CampaignSessionResult ToResult(this DomainCampaignSession session)
    {
        return new CampaignSessionResult(
            session.CampaignSessionId,
            session.SessionStart,
            session.SessionEnd,
            session.Status,
            session.CreatedAt,
            session.EndedAt);
    }
}
