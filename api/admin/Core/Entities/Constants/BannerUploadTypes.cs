namespace Core.Entities.Constants;

public static class BannerUploadTypes
{
    public const string CampaignBanner = "CAMPAIGN_BANNER";
    public const string VoucherDefinitionBanner = "VOUCHER_DEFINITION_BANNER";

    public const long CampaignBannerMaxFileSize = 5 * 1024 * 1024;
    public const long VoucherDefinitionBannerMaxFileSize = 5 * 1024 * 1024;

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        CampaignBanner,
        VoucherDefinitionBanner
    };

    public static long GetMaxFileSize(string type)
    {
        return type switch
        {
            CampaignBanner => CampaignBannerMaxFileSize,
            VoucherDefinitionBanner => VoucherDefinitionBannerMaxFileSize,
            _ => 0
        };
    }
}
