using System.Text.Json;
using DomainCampaign = Core.Entities.Campaign;
using DomainCampaignAction = Core.Entities.CampaignAction;

namespace Core.UseCases.Campaigns;

internal static class CampaignAuditSerializer
{
    public static string Campaign(DomainCampaign campaign)
    {
        return JsonSerializer.Serialize(new
        {
            campaignId = campaign.CampaignId,
            campaignName = campaign.CampaignName,
            description = campaign.Description,
            bannerImageUrl = campaign.BannerImageUrl,
            eventType = campaign.EventType,
            condition = JsonSerializer.Deserialize<JsonElement>(campaign.Condition),
            startDate = campaign.StartDate,
            endDate = campaign.EndDate,
            scheduleCron = campaign.ScheduleCron,
            durationHour = campaign.DurationHour,
            userLimitTotal = campaign.UserLimitTotal,
            userLimitSession = campaign.UserLimitSession,
            status = campaign.Status,
            createdAt = campaign.CreatedAt,
            updatedAt = campaign.UpdatedAt
        });
    }

    public static string Action(DomainCampaignAction action)
    {
        return JsonSerializer.Serialize(new
        {
            actionId = action.ActionId,
            campaignId = action.CampaignId,
            actionType = action.ActionType,
            actionConfig = JsonSerializer.Deserialize<JsonElement>(action.ActionConfig),
            executeOrder = action.ExecuteOrder,
            totalCount = action.TotalCount,
            sessionCount = action.SessionCount,
            usedCount = action.UsedCount,
            createdAt = action.CreatedAt
        });
    }

    public static string Activation(
        DomainCampaign campaign,
        string canonicalCron,
        int generatedSessionCount)
    {
        return JsonSerializer.Serialize(new
        {
            campaignId = campaign.CampaignId,
            campaignName = campaign.CampaignName,
            status = campaign.Status,
            scheduleCron = canonicalCron,
            durationHour = campaign.DurationHour,
            generatedSessionCount = generatedSessionCount,
            activatedAt = campaign.UpdatedAt
        });
    }
}
