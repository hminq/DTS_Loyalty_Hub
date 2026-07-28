using System.Text.Json;

namespace Api.Dtos.Responses.Campaigns;

/// <summary>Represents compatible campaign configuration capabilities.</summary>
public sealed record CampaignOptionsResponseDto(
    IReadOnlyCollection<string> CampaignStatuses,
    CampaignScheduleOptionsResponseDto Schedule,
    IReadOnlyCollection<CampaignEventTypeOptionResponseDto> EventTypes,
    IReadOnlyCollection<CampaignActionTypeOptionResponseDto> ActionTypes);

/// <summary>Represents the fixed campaign scheduling contract.</summary>
public sealed record CampaignScheduleOptionsResponseDto(
    string TimeZone,
    IReadOnlyCollection<string> DaysOfWeek);

/// <summary>Represents one supported campaign event and its compatible actions.</summary>
public sealed record CampaignEventTypeOptionResponseDto(
    string Code,
    CampaignConditionOptionsResponseDto Condition,
    IReadOnlyCollection<CampaignTargetOptionResponseDto> Targets);

/// <summary>Represents condition capabilities for one event type.</summary>
public sealed record CampaignConditionOptionsResponseDto(
    IReadOnlyCollection<string> Combinators,
    IReadOnlyCollection<CampaignConditionFieldOptionResponseDto> Fields,
    IReadOnlyCollection<CampaignConditionPresetOptionResponseDto> Presets);

/// <summary>Represents one condition field supported by an event type.</summary>
public sealed record CampaignConditionFieldOptionResponseDto(
    string Code,
    string DataType,
    IReadOnlyCollection<string> Operators,
    IReadOnlyCollection<string> Options);

/// <summary>Represents one selectable condition preset.</summary>
public sealed record CampaignConditionPresetOptionResponseDto(
    string Code,
    JsonElement Condition);

/// <summary>Represents one selectable event target.</summary>
public sealed record CampaignTargetOptionResponseDto(
    string Code,
    string TargetKind,
    JsonElement Applicability);

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
