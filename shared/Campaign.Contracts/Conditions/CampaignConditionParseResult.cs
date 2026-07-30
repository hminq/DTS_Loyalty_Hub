using Campaign.Contracts.Definitions;

namespace Campaign.Contracts.Conditions;

public sealed record CampaignConditionParseResult(
    CampaignCondition? Condition,
    string? CanonicalJson,
    IReadOnlyList<CampaignDefinitionError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
