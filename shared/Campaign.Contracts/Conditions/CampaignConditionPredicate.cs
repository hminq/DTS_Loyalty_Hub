namespace Campaign.Contracts.Conditions;

public sealed record CampaignConditionPredicate(
    string Field,
    string Operator,
    IReadOnlyList<string> Values);
