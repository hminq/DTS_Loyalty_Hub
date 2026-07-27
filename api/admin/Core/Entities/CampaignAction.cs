using Core.Exceptions;

namespace Core.Entities;

public sealed class CampaignAction
{
    private CampaignAction(
        Guid actionId,
        Guid campaignId,
        string actionType,
        string actionConfig,
        int executeOrder,
        int? totalCount,
        int? sessionCount,
        decimal? totalAmount,
        decimal? sessionAmount,
        int usedCount,
        decimal usedAmount,
        DateTime createdAt)
    {
        ActionId = actionId;
        CampaignId = campaignId;
        ActionType = actionType;
        ActionConfig = actionConfig;
        ExecuteOrder = executeOrder;
        TotalCount = totalCount;
        SessionCount = sessionCount;
        TotalAmount = totalAmount;
        SessionAmount = sessionAmount;
        UsedCount = usedCount;
        UsedAmount = usedAmount;
        CreatedAt = createdAt;
    }

    public Guid ActionId { get; }
    public Guid CampaignId { get; }
    public string ActionType { get; private set; }
    public string ActionConfig { get; private set; }
    public int ExecuteOrder { get; private set; }
    public int? TotalCount { get; private set; }
    public int? SessionCount { get; private set; }
    public decimal? TotalAmount { get; private set; }
    public decimal? SessionAmount { get; private set; }
    public int UsedCount { get; }
    public decimal UsedAmount { get; }
    public DateTime CreatedAt { get; }

    public static CampaignAction Create(
        Guid campaignId,
        string actionType,
        string actionConfig,
        int executeOrder,
        int? totalCount,
        int? sessionCount,
        decimal? totalAmount,
        decimal? sessionAmount,
        DateTime now)
    {
        if (campaignId == Guid.Empty)
        {
            throw ValidationError("CAMPAIGN_ID_REQUIRED");
        }

        Validate(
            actionType,
            actionConfig,
            executeOrder,
            totalCount,
            sessionCount,
            totalAmount,
            sessionAmount);

        return new CampaignAction(
            Guid.NewGuid(),
            campaignId,
            actionType,
            actionConfig,
            executeOrder,
            totalCount,
            sessionCount,
            totalAmount,
            sessionAmount,
            0,
            0,
            now);
    }

    public static CampaignAction Restore(
        Guid actionId,
        Guid campaignId,
        string actionType,
        string actionConfig,
        int executeOrder,
        int? totalCount,
        int? sessionCount,
        decimal? totalAmount,
        decimal? sessionAmount,
        int usedCount,
        decimal usedAmount,
        DateTime createdAt)
    {
        if (actionId == Guid.Empty)
        {
            throw ValidationError("CAMPAIGN_ACTION_ID_REQUIRED");
        }

        Validate(
            actionType,
            actionConfig,
            executeOrder,
            totalCount,
            sessionCount,
            totalAmount,
            sessionAmount);

        return new CampaignAction(
            actionId,
            campaignId,
            actionType,
            actionConfig,
            executeOrder,
            totalCount,
            sessionCount,
            totalAmount,
            sessionAmount,
            usedCount,
            usedAmount,
            createdAt);
    }

    public void Update(
        string actionType,
        string actionConfig,
        int executeOrder,
        int? totalCount,
        int? sessionCount,
        decimal? totalAmount,
        decimal? sessionAmount)
    {
        Validate(
            actionType,
            actionConfig,
            executeOrder,
            totalCount,
            sessionCount,
            totalAmount,
            sessionAmount);

        ActionType = actionType;
        ActionConfig = actionConfig;
        ExecuteOrder = executeOrder;
        TotalCount = totalCount;
        SessionCount = sessionCount;
        TotalAmount = totalAmount;
        SessionAmount = sessionAmount;
    }

    private static void Validate(
        string actionType,
        string actionConfig,
        int executeOrder,
        int? totalCount,
        int? sessionCount,
        decimal? totalAmount,
        decimal? sessionAmount)
    {
        if (string.IsNullOrWhiteSpace(actionType))
        {
            throw ValidationError("CAMPAIGN_ACTION_TYPE_INVALID");
        }

        if (string.IsNullOrWhiteSpace(actionConfig))
        {
            throw ValidationError("CAMPAIGN_ACTION_CONFIG_INVALID");
        }

        if (executeOrder <= 0)
        {
            throw ValidationError("CAMPAIGN_ACTION_ORDER_INVALID");
        }

        if (totalCount is < 0 ||
            sessionCount is < 0 ||
            totalCount.HasValue &&
            sessionCount.HasValue &&
            sessionCount > totalCount ||
            totalAmount is < 0 ||
            sessionAmount is < 0 ||
            totalAmount.HasValue && HasMoreThanTwoDecimalPlaces(totalAmount.Value) ||
            sessionAmount.HasValue && HasMoreThanTwoDecimalPlaces(sessionAmount.Value) ||
            totalAmount.HasValue &&
            sessionAmount.HasValue &&
            sessionAmount > totalAmount)
        {
            throw ValidationError("CAMPAIGN_LIMIT_INVALID");
        }
    }

    private static bool HasMoreThanTwoDecimalPlaces(decimal value)
    {
        return decimal.Round(value, 2) != value;
    }

    private static DomainException ValidationError(string code)
    {
        return new DomainException(code, DomainErrorType.Validation);
    }
}
