namespace Campaign.Contracts.Definitions;

public sealed record CampaignEventDefinition(
    string Code,
    string PrimaryTargetSelector,
    IReadOnlyList<CampaignConditionFieldDefinition> ConditionFields,
    IReadOnlyList<CampaignTargetDefinition> Targets);
