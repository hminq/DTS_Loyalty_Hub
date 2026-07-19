using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Api.Localization;

public sealed class ApiMessageResolver(
    IStringLocalizer<ApiMessages> localizer,
    ILogger<ApiMessageResolver> logger)
{
    public string Resolve(
        string errorCode,
        IReadOnlyDictionary<string, object>? placeholders = null)
    {
        var localizedMessage = localizer[errorCode];
        if (localizedMessage.ResourceNotFound)
        {
            logger.LogWarning(
                "Missing localized API message for error code {ErrorCode} and culture {Culture}",
                errorCode,
                CultureInfo.CurrentUICulture.Name);

            return localizer["INTERNAL_SERVER_ERROR"].Value;
        }

        return ReplacePlaceholders(localizedMessage.Value, placeholders);
    }

    public string Resolve(
        string errorCode,
        IReadOnlyList<object> arguments)
    {
        var localizedMessage = localizer[errorCode];
        if (localizedMessage.ResourceNotFound)
        {
            logger.LogWarning(
                "Missing localized API message for error code {ErrorCode} and culture {Culture}",
                errorCode,
                CultureInfo.CurrentUICulture.Name);

            return localizer["INTERNAL_SERVER_ERROR"].Value;
        }

        return arguments.Count > 0
            ? string.Format(CultureInfo.CurrentUICulture, localizedMessage.Value, arguments.ToArray())
            : localizedMessage.Value;
    }

    private static string ReplacePlaceholders(
        string message,
        IReadOnlyDictionary<string, object>? placeholders)
    {
        if (placeholders is null)
        {
            return message;
        }

        return placeholders.Aggregate(
            message,
            (current, placeholder) => current.Replace(
                $"{{{placeholder.Key}}}",
                Convert.ToString(placeholder.Value, CultureInfo.CurrentUICulture),
                StringComparison.Ordinal));
    }
}
