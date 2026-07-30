namespace Core.Entities;

public sealed record NotificationTemplateVariable(
    string Name,
    string SourceType,
    string? SourceKey,
    string? FixedValue,
    string? DefaultValue,
    string? Description = null);
