namespace Core.Entities.Constants;

public static class NotificationCodes
{
    public const string CustomerTierUpgraded = "CUSTOMER_TIER_UPGRADED";
    public const string CustomerVoucherIssued = "CUSTOMER_VOUCHER_ISSUED";
    public const string CustomerVoucherExpiring = "CUSTOMER_VOUCHER_EXPIRING";
    public const string CampaignRewardReceived = "CAMPAIGN_REWARD_RECEIVED";

    public static readonly IReadOnlyCollection<NotificationCodeDefinition> All =
    [
        new(CustomerTierUpgraded, "Customer tier upgraded", "Notification after a customer tier upgrade", [
            new("customerName", "Customer name", "STRING"),
            new("oldTierName", "Old tier name", "STRING"),
            new("newTierName", "New tier name", "STRING"),
            new("currentTierName", "Current tier name", "STRING"),
            new("nextTierName", "Next tier name", "STRING"),
            new("pointsRequired", "Points required", "STRING")
        ]),
        new(CustomerVoucherIssued, "Customer voucher issued", "Notification after a voucher is issued", [
            new("customerName", "Customer name", "STRING"),
            new("voucherCode", "Voucher code", "STRING")
        ]),
        new(CustomerVoucherExpiring, "Customer voucher expiring", "Notification before a voucher expires", [
            new("customerName", "Customer name", "STRING"),
            new("voucherCode", "Voucher code", "STRING"),
            new("expiredAt", "Expired at", "STRING")
        ]),
        new(CampaignRewardReceived, "Campaign reward received", "Notification after a campaign reward is received", [
            new("customerName", "Customer name", "STRING"),
            new("campaignName", "Campaign name", "STRING"),
            new("rewardName", "Reward name", "STRING")
        ])
    ];

    public static bool IsDefined(string? code) =>
        !string.IsNullOrWhiteSpace(code) && All.Any(x => x.Code == code.Trim());

    public static NotificationCodeDefinition? Find(string code) =>
        All.FirstOrDefault(x => x.Code == code.Trim());
}

public sealed record NotificationCodeDefinition(
    string Code,
    string DisplayName,
    string? Description,
    IReadOnlyCollection<NotificationInputFieldDefinition> InputFields);

public sealed record NotificationInputFieldDefinition(
    string Key,
    string DisplayName,
    string DataType,
    string? Description = null);
