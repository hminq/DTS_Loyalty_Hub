namespace Core.UseCases.Campaigns.Results;

public sealed record CampaignOptionsResult(
    IReadOnlyCollection<string> CampaignStatuses,
    CampaignScheduleOptionsResult Schedule,
    IReadOnlyCollection<CampaignEventTypeOptionResult> EventTypes,
    IReadOnlyCollection<CampaignActionTypeOptionResult> ActionTypes);

public sealed record CampaignScheduleOptionsResult(
    string TimeZone,
    IReadOnlyCollection<string> DaysOfWeek);

public sealed record CampaignEventTypeOptionResult(
    string Code,
    CampaignConditionOptionsResult Condition,
    IReadOnlyCollection<CampaignTargetOptionResult> Targets);

public sealed record CampaignConditionOptionsResult(
    IReadOnlyCollection<string> Combinators,
    IReadOnlyCollection<CampaignConditionFieldOptionResult> Fields,
    IReadOnlyCollection<CampaignConditionPresetOptionResult> Presets);

public sealed record CampaignConditionFieldOptionResult(
    string Code,
    string DataType,
    IReadOnlyCollection<string> Operators,
    IReadOnlyCollection<string> Options);

public sealed record CampaignConditionPresetOptionResult(
    string Code,
    string Condition);

public sealed record CampaignTargetOptionResult(
    string Code,
    string TargetKind,
    string Applicability);

public sealed record CampaignActionTypeOptionResult(
    string Code,
    string RequiredTargetKind,
    IReadOnlyCollection<CampaignParameterFieldOptionResult> Parameters);

public sealed record CampaignParameterFieldOptionResult(
    string Code,
    string DataType,
    bool Required,
    decimal? MinimumExclusive,
    decimal? Maximum,
    int? Scale);
