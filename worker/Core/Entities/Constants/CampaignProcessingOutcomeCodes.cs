namespace Core.Entities.Constants;

public static class CampaignProcessingOutcomeCodes
{
    public const string ConditionNotMatched = "CAMPAIGN_CONDITION_NOT_MATCHED";
    public const string UserTotalLimitReached = "CAMPAIGN_USER_TOTAL_LIMIT_REACHED";
    public const string UserSessionLimitReached = "CAMPAIGN_USER_SESSION_LIMIT_REACHED";
    public const string ActionTotalLimitReached = "ACTION_TOTAL_LIMIT_REACHED";
    public const string ActionSessionLimitReached = "ACTION_SESSION_LIMIT_REACHED";
    public const string CampaignCancelled = "CAMPAIGN_CANCELLED";
}
