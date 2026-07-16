namespace Core.UseCases.Notifications.Results;

public record NotificationEventTypeResult(
    Guid NotificationEventTypeId,
    string EventTypeCode,
    string DisplayName,
    string? Description,
    string AvailableVariables);
