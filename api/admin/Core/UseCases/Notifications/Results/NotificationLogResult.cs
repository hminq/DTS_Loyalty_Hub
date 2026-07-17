namespace Core.UseCases.Notifications.Results;

public record NotificationLogResult(
    Guid LogId,
    Guid? TemplateId,
    string EventTypeCode,
    string Channel,
    Guid CustomerId,
    string? RenderedTitle,
    string RenderedBody,
    string? VariablesSnapshot,
    string Status,
    DateTime CreatedAt,
    DateTime? SentAt);
