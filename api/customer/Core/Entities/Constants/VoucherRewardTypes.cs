namespace Core.Entities.Constants;

public static class VoucherRewardTypes
{
    public const string Fixed = "FIXED";
    public const string Percent = "PERCENT";
    public const string Gift = "GIFT";

    public static IReadOnlyCollection<string> All { get; } =
    [
        Fixed,
        Percent,
        Gift
    ];

    public static bool IsDefined(string value)
    {
        var normalizedValue = value.Trim().ToUpperInvariant();
        return All.Contains(normalizedValue);
    }
}
