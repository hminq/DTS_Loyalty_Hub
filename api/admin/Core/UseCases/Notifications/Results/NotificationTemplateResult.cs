namespace Core.UseCases.Notifications.Results;

public record NotificationTemplateResult(
    Guid TemplateId,
    Guid NotificationEventTypeId,
    string EventTypeCode,
    string EventTypeDisplayName,
    string Channel,
    string Language,
    string Name,
    string TitleTemplate,
    string BodyTemplate,
    bool IsActive,
    Guid? CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);
