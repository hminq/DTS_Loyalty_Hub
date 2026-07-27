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
    IReadOnlyCollection<string> ActionTypes);

public sealed record CampaignConditionOptionsResult(
    IReadOnlyCollection<CampaignCapabilityOptionResult> Sources);

public sealed record CampaignActionTypeOptionResult(
    string Code,
    IReadOnlyCollection<CampaignCapabilityOptionResult> CalculationTypes,
    IReadOnlyCollection<CampaignCapabilityOptionResult> Recipients);

public sealed record CampaignCapabilityOptionResult(string Code, bool Supported);
