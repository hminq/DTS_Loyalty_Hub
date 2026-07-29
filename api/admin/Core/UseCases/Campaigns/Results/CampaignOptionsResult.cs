namespace Core.UseCases.Campaigns.Results;

public sealed record CampaignOptionsResult(
    IReadOnlyCollection<string> CampaignStatuses,
    CampaignScheduleOptionsResult Schedule,
    IReadOnlyCollection<CampaignEventTypeVersionOptionResult> EventTypeVersions,
    IReadOnlyCollection<CampaignActionTypeOptionResult> ActionTypes);

public sealed record CampaignScheduleOptionsResult(
    string TimeZone,
    IReadOnlyCollection<string> DaysOfWeek);

public sealed record CampaignEventTypeVersionOptionResult(
    Guid EventTypeId,
    Guid EventTypeVersionId,
    string Code,
    string RoutingKey,
    string Name,
    int Version,
    CampaignConditionOptionsResult Condition,
    IReadOnlyCollection<CampaignTargetOptionResult> Targets);

public sealed record CampaignConditionOptionsResult(
    IReadOnlyCollection<string> Combinators,
    IReadOnlyCollection<CampaignConditionFieldOptionResult> Fields);

public sealed record CampaignConditionFieldOptionResult(
    string Code,
    string DataType,
    string? Format,
    bool Required,
    IReadOnlyCollection<string> Operators,
    IReadOnlyCollection<string> Options);

public sealed record CampaignTargetOptionResult(
    string Selector,
    string TargetKind,
    string IdField);

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
