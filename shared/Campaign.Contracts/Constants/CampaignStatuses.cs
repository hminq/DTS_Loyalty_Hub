namespace Campaign.Contracts.Constants;

public static class CampaignStatuses
{
    public const string Draft = "DRAFT";
    public const string Active = "ACTIVE";
    public const string Ended = "ENDED";
    public const string Cancelled = "CANCELLED";

    public static IReadOnlyList<string> All { get; } =
    [
        Draft,
        Active,
        Ended,
        Cancelled
    ];

    public static bool IsDefined(string value)
    {
        return All.Contains(value.Trim().ToUpperInvariant(), StringComparer.Ordinal);
    }
}
