using System.Text.Json;

namespace Api.Dtos.Responses.Campaigns;

/// <summary>Represents compatible campaign configuration capabilities.</summary>
public sealed record CampaignOptionsResponseDto(
    IReadOnlyCollection<string> CampaignStatuses,
    CampaignScheduleOptionsResponseDto Schedule,
    IReadOnlyCollection<CampaignEventTypeVersionOptionResponseDto> EventTypeVersions,
    IReadOnlyCollection<CampaignActionTypeOptionResponseDto> ActionTypes);

/// <summary>Represents the fixed campaign scheduling contract.</summary>
public sealed record CampaignScheduleOptionsResponseDto(
    string TimeZone,
    IReadOnlyCollection<string> DaysOfWeek);

/// <summary>Represents one supported campaign event and its compatible actions.</summary>
public sealed record CampaignEventTypeVersionOptionResponseDto(
    Guid EventTypeId,
    Guid EventTypeVersionId,
    string Code,
    string RoutingKey,
    string Name,
    int Version,
    CampaignConditionOptionsResponseDto Condition,
    IReadOnlyCollection<CampaignTargetOptionResponseDto> Targets);

/// <summary>Represents condition capabilities for one event type.</summary>
public sealed record CampaignConditionOptionsResponseDto(
    IReadOnlyCollection<string> Combinators,
    IReadOnlyCollection<CampaignConditionFieldOptionResponseDto> Fields);

/// <summary>Represents one condition field supported by an event type.</summary>
public sealed record CampaignConditionFieldOptionResponseDto(
    string Code,
    string DataType,
    string? Format,
    bool Required,
    IReadOnlyCollection<string> Operators,
    IReadOnlyCollection<string> Options);

/// <summary>Represents one selectable event target.</summary>
public sealed record CampaignTargetOptionResponseDto(
    string Selector,
    string TargetKind,
    string IdField);

/// <summary>Represents one action type and its compatible configuration values.</summary>
public sealed record CampaignActionTypeOptionResponseDto(
    string Code,
    string RequiredTargetKind,
    IReadOnlyCollection<CampaignParameterFieldOptionResponseDto> Parameters);

/// <summary>Represents one action parameter field.</summary>
public sealed record CampaignParameterFieldOptionResponseDto(
    string Code,
    string DataType,
    bool Required,
    decimal? MinimumExclusive,
    decimal? Maximum,
    int? Scale);
