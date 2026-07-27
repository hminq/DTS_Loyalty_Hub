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
    IReadOnlyCollection<string> ActionTypes);

/// <summary>Represents condition capabilities for one event type.</summary>
public sealed record CampaignConditionOptionsResponseDto(
    IReadOnlyCollection<CampaignConditionOptionResponseDto> Options);

/// <summary>
/// Represents one selectable condition and the event-payload sources it matches.
/// An empty Sources collection means the condition matches all sources.
/// </summary>
public sealed record CampaignConditionOptionResponseDto(
    string Code,
    IReadOnlyCollection<string> Sources,
    bool Supported);

/// <summary>Represents one action type and its compatible configuration values.</summary>
public sealed record CampaignActionTypeOptionResponseDto(
    string Code,
    IReadOnlyCollection<CampaignCapabilityOptionResponseDto> CalculationTypes,
    IReadOnlyCollection<CampaignCapabilityOptionResponseDto> Recipients);

/// <summary>Represents one capability and whether phase 1 can execute it.</summary>
public sealed record CampaignCapabilityOptionResponseDto(string Code, bool Supported);
