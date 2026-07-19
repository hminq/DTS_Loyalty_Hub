using Core.Exceptions;

namespace Core.Entities;

public class NotificationTemplate
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 255;

    private NotificationTemplate(
        Guid templateId,
        Guid notificationEventTypeId,
        string channel,
        string language,
        string name,
        string titleTemplate,
        string bodyTemplate,
        bool isActive,
        Guid? createdBy,
        DateTime createdAt,
        DateTime updatedAt)
    {
        TemplateId = templateId;
        NotificationEventTypeId = notificationEventTypeId;
        Channel = channel;
        Language = language;
        Name = name;
        TitleTemplate = titleTemplate;
        BodyTemplate = bodyTemplate;
        IsActive = isActive;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid TemplateId { get; private set; }
    public Guid NotificationEventTypeId { get; private set; }
    public string Channel { get; private set; }
    public string Language { get; private set; }
    public string Name { get; private set; }
    public string TitleTemplate { get; private set; }
    public string BodyTemplate { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static NotificationTemplate Create(
        Guid notificationEventTypeId,
        string channel,
        string language,
        string name,
        string titleTemplate,
        string bodyTemplate,
        Guid? createdBy)
    {
        ValidateName(name);
        ValidateTemplateContent(titleTemplate, bodyTemplate);
        
        return new NotificationTemplate(
            Guid.NewGuid(),
            notificationEventTypeId,
            channel,
            language,
            name.Trim(),
            titleTemplate.Trim(),
            bodyTemplate.Trim(),
            true, // Default is active
            createdBy,
            DateTime.UtcNow,
            DateTime.UtcNow);
    }

    public static NotificationTemplate Restore(
        Guid templateId,
        Guid notificationEventTypeId,
        string channel,
        string language,
        string name,
        string titleTemplate,
        string bodyTemplate,
        bool isActive,
        Guid? createdBy,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new NotificationTemplate(
            templateId,
            notificationEventTypeId,
            channel,
            language,
            name,
            titleTemplate,
            bodyTemplate,
            isActive,
            createdBy,
            createdAt,
            updatedAt);
    }

    public void Update(
        Guid notificationEventTypeId,
        string channel,
        string language,
        string name,
        string titleTemplate,
        string bodyTemplate,
        bool isActive)
    {
        ValidateName(name);
        ValidateTemplateContent(titleTemplate, bodyTemplate);

        NotificationEventTypeId = notificationEventTypeId;
        Channel = channel;
        Language = language;
        Name = name.Trim();
        TitleTemplate = titleTemplate.Trim();
        BodyTemplate = bodyTemplate.Trim();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleStatus()
    {
        IsActive = !IsActive;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ValidationError("TEMPLATE_NAME_REQUIRED");

        var length = name.Trim().Length;
        if (length < MinNameLength || length > MaxNameLength)
            throw ValidationError("TEMPLATE_NAME_LENGTH_INVALID");
    }

    private static void ValidateTemplateContent(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw ValidationError("TEMPLATE_TITLE_REQUIRED");
            
        if (string.IsNullOrWhiteSpace(body))
            throw ValidationError("TEMPLATE_BODY_REQUIRED");
    }

    private static DomainException ValidationError(string errorCode)
    {
        return new DomainException(errorCode, DomainErrorType.Validation);
    }
}
