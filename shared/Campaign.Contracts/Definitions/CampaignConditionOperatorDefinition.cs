namespace Campaign.Contracts.Definitions;

public sealed record CampaignConditionOperatorDefinition(
    string Code,
    IReadOnlyList<string> SupportedFieldTypes);
