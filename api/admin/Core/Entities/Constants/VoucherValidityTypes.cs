namespace Core.Entities.Constants;

public static class VoucherValidityTypes
{
    public const string Fixed = "FIXED";
    public const string Dynamic = "DYNAMIC";

    public static bool IsDefined(string value)
    {
        var normalizedValue = Normalize(value);

        return normalizedValue is Fixed or Dynamic;
    }

    public static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
