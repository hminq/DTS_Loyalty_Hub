namespace Core.UseCases.Notifications.Results;

public record NotificationTemplateResult(
    Guid TemplateId,
    string NotificationCode,
    string Channel,
    string Language,
    string Name,
    string TitleTemplate,
    string BodyTemplate,
    IReadOnlyCollection<Core.Entities.NotificationTemplateVariable> Variables,
    bool IsActive,
    Guid? CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);
