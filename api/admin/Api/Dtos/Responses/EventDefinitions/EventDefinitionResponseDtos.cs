using System.Text.Json;

namespace Api.Dtos.Responses.EventDefinitions;

public sealed record EventDefinitionListItemResponseDto(
    Guid EventTypeId,
    string Code,
    string RoutingKey,
    string Name,
    string? Description,
    string Status,
    int? LatestVersion,
    int? LatestPublishedVersion,
    int? DraftVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record EventDefinitionDetailResponseDto(
    Guid EventTypeId,
    string Code,
    string RoutingKey,
    string Name,
    string? Description,
    string Status,
    bool CanEditIdentity,
    bool CanCreateDraft,
    bool CanRetire,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<EventDefinitionVersionSummaryResponseDto> Versions);

public sealed record EventDefinitionVersionSummaryResponseDto(
    Guid EventTypeVersionId,
    int Version,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? PublishedAt);

public sealed record EventDefinitionVersionDetailResponseDto(
    Guid EventTypeVersionId,
    Guid EventTypeId,
    string EventTypeCode,
    int Version,
    string Status,
    JsonElement PayloadSchema,
    bool CanEdit,
    bool CanPublish,
    bool CanClone,
    bool CanRetire,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? PublishedAt);

public sealed record EventDefinitionOptionsResponseDto(
    IReadOnlyCollection<EventDefinitionFieldTypeOptionResponseDto> FieldTypes,
    IReadOnlyCollection<EventDefinitionTargetKindOptionResponseDto> TargetKinds,
    IReadOnlyCollection<string> EventTypeStatuses,
    IReadOnlyCollection<string> VersionStatuses,
    EventDefinitionLimitsResponseDto Limits);

public sealed record EventDefinitionFieldTypeOptionResponseDto(
    string Code,
    IReadOnlyCollection<string> Formats,
    IReadOnlyCollection<string> Operators);

public sealed record EventDefinitionTargetKindOptionResponseDto(
    string Code,
    string IdentityFieldType,
    string IdentityFieldFormat);

public sealed record EventDefinitionLimitsResponseDto(
    int MaximumFields,
    int MaximumTargets,
    int MaximumSchemaBytes);
