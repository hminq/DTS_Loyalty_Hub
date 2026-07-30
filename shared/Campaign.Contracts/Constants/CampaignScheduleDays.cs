namespace Campaign.Contracts.Constants;

public static class CampaignScheduleDays
{
    public const string Monday = "MON";
    public const string Tuesday = "TUE";
    public const string Wednesday = "WED";
    public const string Thursday = "THU";
    public const string Friday = "FRI";
    public const string Saturday = "SAT";
    public const string Sunday = "SUN";

    public static IReadOnlyList<string> CanonicalOrder { get; } =
    [
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    ];
}
