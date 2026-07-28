namespace Campaign.Contracts.Definitions;

public sealed record CampaignConditionFieldDefinition(
    string Code,
    string DataType,
    IReadOnlyList<string> Operators,
    IReadOnlyList<string> Options,
    bool Required);
