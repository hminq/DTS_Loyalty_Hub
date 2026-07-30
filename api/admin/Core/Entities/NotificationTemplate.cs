using Core.Exceptions;
using Core.Entities.Constants;
using System.Text.RegularExpressions;

namespace Core.Entities;

public class NotificationTemplate
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 255;

    private NotificationTemplate(
        Guid templateId,
        string notificationCode,
        string channel,
        string language,
        string name,
        string titleTemplate,
        string bodyTemplate,
        IReadOnlyCollection<NotificationTemplateVariable> variables,
        bool isActive,
        Guid? createdBy,
        DateTime createdAt,
        DateTime updatedAt)
    {
        TemplateId = templateId;
        NotificationCode = notificationCode;
        Channel = channel;
        Language = language;
        Name = name;
        TitleTemplate = titleTemplate;
        BodyTemplate = bodyTemplate;
        Variables = variables;
        IsActive = isActive;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid TemplateId { get; private set; }
    public string NotificationCode { get; private set; }
    public string Channel { get; private set; }
    public string Language { get; private set; }
    public string Name { get; private set; }
    public string TitleTemplate { get; private set; }
    public string BodyTemplate { get; private set; }
    public IReadOnlyCollection<NotificationTemplateVariable> Variables { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static NotificationTemplate Create(
        string notificationCode,
        string channel,
        string language,
        string name,
        string titleTemplate,
        string bodyTemplate,
        IReadOnlyCollection<NotificationTemplateVariable> variables,
        Guid? createdBy)
    {
        ValidateName(name);
        ValidateTemplateContent(titleTemplate, bodyTemplate);
        ValidateContract(notificationCode, variables, titleTemplate, bodyTemplate);
        
        return new NotificationTemplate(
            Guid.NewGuid(),
            notificationCode.Trim(),
            channel,
            language,
            name.Trim(),
            titleTemplate.Trim(),
            bodyTemplate.Trim(),
            variables,
            false, // Default is inactive
            createdBy,
            DateTime.UtcNow,
            DateTime.UtcNow);
    }

    public static NotificationTemplate Restore(
        Guid templateId,
        string notificationCode,
        string channel,
        string language,
        string name,
        string titleTemplate,
        string bodyTemplate,
        IReadOnlyCollection<NotificationTemplateVariable> variables,
        bool isActive,
        Guid? createdBy,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new NotificationTemplate(
            templateId,
            notificationCode,
            channel,
            language,
            name,
            titleTemplate,
            bodyTemplate,
            variables,
            isActive,
            createdBy,
            createdAt,
            updatedAt);
    }

    public void Update(
        string notificationCode,
        string channel,
        string language,
        string name,
        string titleTemplate,
        string bodyTemplate,
        IReadOnlyCollection<NotificationTemplateVariable> variables,
        bool isActive)
    {
        ValidateName(name);
        ValidateTemplateContent(titleTemplate, bodyTemplate);
        ValidateContract(notificationCode, variables, titleTemplate, bodyTemplate);

        NotificationCode = notificationCode.Trim();
        Channel = channel;
        Language = language;
        Name = name.Trim();
        TitleTemplate = titleTemplate.Trim();
        BodyTemplate = bodyTemplate.Trim();
        Variables = variables;
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

    private static void ValidateContract(
        string notificationCode,
        IReadOnlyCollection<NotificationTemplateVariable> variables,
        string title,
        string body)
    {
        if (!NotificationCodes.IsDefined(notificationCode))
            throw ValidationError("NOTIFICATION_CODE_INVALID");

        var duplicateNames = variables
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);
        if (duplicateNames)
            throw ValidationError("VARIABLE_NAME_DUPLICATED");

        foreach (var variable in variables)
        {
            if (!Regex.IsMatch(variable.Name, "^[A-Za-z][A-Za-z0-9_]*$"))
                throw ValidationError("VARIABLE_NAME_INVALID");

            if (variable.Description?.Length > 500)
                throw ValidationError("VARIABLE_DESCRIPTION_TOO_LONG");

            if (!NotificationVariableSourceTypes.IsDefined(variable.SourceType))
                throw ValidationError("VARIABLE_SOURCE_TYPE_INVALID");

            if (NotificationVariableSourceTypes.RequiresSourceKey(variable.SourceType))
            {
                if (string.IsNullOrWhiteSpace(variable.SourceKey))
                    throw ValidationError("VARIABLE_SOURCE_KEY_REQUIRED");
            }
            else if (variable.SourceType == NotificationVariableSourceTypes.LegacyFixed
                && string.IsNullOrWhiteSpace(variable.FixedValue))
            {
                throw ValidationError("VARIABLE_FIXED_VALUE_REQUIRED");
            }
        }

        var configuredNames = variables
            .Select(x => x.Name)
            .ToHashSet(StringComparer.Ordinal);
        var placeholders = Regex.Matches($"{title}\n{body}", "\\{\\{([A-Za-z][A-Za-z0-9_]*)\\}\\}")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal);
        if (placeholders.Any(name => !configuredNames.Contains(name)))
            throw ValidationError("TEMPLATE_PLACEHOLDER_UNDEFINED");
    }

    private static DomainException ValidationError(string errorCode)
    {
        return new DomainException(errorCode, DomainErrorType.Validation);
    }
}
