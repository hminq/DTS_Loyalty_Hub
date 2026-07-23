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
        var normalizedValue = Normalize(value);

        return normalizedValue is Fixed or Percent or Gift;
    }

    public static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
