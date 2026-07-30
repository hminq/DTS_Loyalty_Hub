namespace Core.Entities.Constants;

public static class NotificationVariableSourceTypes
{
    public const string Input = "INPUT";
    public const string System = "SYSTEM";
    public const string LegacyRequest = "REQUEST";
    public const string LegacyFixed = "FIXED";

    public static bool IsDefined(string? value) =>
        value is Input or System or LegacyRequest or LegacyFixed;

    public static bool RequiresSourceKey(string value) =>
        value is Input or System or LegacyRequest;
}

public static class NotificationSystemFields
{
    public static readonly IReadOnlyCollection<NotificationSystemFieldDefinition> All =
    [
        new("currentDate", "Current date", "DATE"),
        new("currentTime", "Current time", "TIME"),
        new("supportPhone", "Support phone", "STRING"),
        new("supportEmail", "Support email", "STRING")
    ];
}

public sealed record NotificationSystemFieldDefinition(
    string Key,
    string DisplayName,
    string DataType);
