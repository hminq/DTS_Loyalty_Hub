namespace Core.Entities.Campaigns;

public sealed record CampaignRewardAction(
    Guid ActionId,
    string ActionType,
    string ActionConfigJson,
    int ExecuteOrder,
    int? TotalCount,
    int? SessionCount,
    int UsedCount);
