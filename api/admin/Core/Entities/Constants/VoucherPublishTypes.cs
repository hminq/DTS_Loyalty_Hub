namespace Core.Entities.Constants;

public static class VoucherPublishTypes
{
    public const string Public = "PUBLIC";
    public const string Private = "PRIVATE";

    public static bool IsDefined(string value)
    {
        var normalizedValue = Normalize(value);

        return normalizedValue is Public or Private;
    }

    public static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
