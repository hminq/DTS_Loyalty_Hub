namespace Core.Entities.Constants;

public static class AuditEntityTypes
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public const string Role = "Role";
    public const string TierConfig = "TierConfig";
    public const string VoucherDefinition = "VoucherDefinition";
    public const string NotificationTemplate = "NotificationTemplate";
    public const string Campaign = "Campaign";
    public const string CampaignAction = "CampaignAction";
    public const string EventType = "EventType";
    public const string EventTypeVersion = "EventTypeVersion";

    public static IReadOnlyCollection<string> All { get; } =
    [
        Admin,
        Customer,
        Role,
        TierConfig,
        VoucherDefinition,
        NotificationTemplate,
        Campaign,
        CampaignAction,
        EventType,
        EventTypeVersion
    ];
}
