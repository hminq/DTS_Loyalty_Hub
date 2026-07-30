namespace Core.UseCases.Notifications.Results;

public sealed record NotificationCodeResult(
    string Code,
    string DisplayName,
    string? Description,
    IReadOnlyCollection<NotificationInputFieldResult> InputFields,
    IReadOnlyCollection<NotificationSystemFieldResult> SystemFields);

public sealed record NotificationInputFieldResult(
    string Key,
    string DisplayName,
    string DataType,
    string? Description);

public sealed record NotificationSystemFieldResult(
    string Key,
    string DisplayName,
    string DataType);
