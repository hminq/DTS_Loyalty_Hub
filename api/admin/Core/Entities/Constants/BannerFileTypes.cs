using Core.Exceptions;

namespace Core.Entities.Constants;

public static class BannerFileTypes
{
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string WebP = "image/webp";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Jpeg,
        Png,
        WebP
    };

    public static string GetExtension(string contentType)
    {
        if (string.Equals(contentType, Jpeg, StringComparison.OrdinalIgnoreCase))
        {
            return ".jpg";
        }

        if (string.Equals(contentType, Png, StringComparison.OrdinalIgnoreCase))
        {
            return ".png";
        }

        if (string.Equals(contentType, WebP, StringComparison.OrdinalIgnoreCase))
        {
            return ".webp";
        }

        throw new DomainException(
            "BANNER_FILE_TYPE_INVALID",
            DomainErrorType.Validation);
    }
}
