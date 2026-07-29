namespace Core.UseCases.EventDefinitions.Results;

public sealed record EventDefinitionListItemResult(
    Guid EventTypeId,
    string Code,
    string RoutingKey,
    string Name,
    string? Description,
    string Status,
    int? LatestVersion,
    int? LatestPublishedVersion,
    int? DraftVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record EventDefinitionDetailResult(
    Guid EventTypeId,
    string Code,
    string RoutingKey,
    string Name,
    string? Description,
    string Status,
    bool CanEditIdentity,
    bool CanCreateDraft,
    bool CanRetire,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<EventDefinitionVersionSummaryResult> Versions);

public sealed record EventDefinitionVersionSummaryResult(
    Guid EventTypeVersionId,
    int Version,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);

public sealed record EventDefinitionVersionDetailResult(
    Guid EventTypeVersionId,
    Guid EventTypeId,
    string EventTypeCode,
    int Version,
    string Status,
    string PayloadSchema,
    bool CanEdit,
    bool CanPublish,
    bool CanClone,
    bool CanRetire,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);
