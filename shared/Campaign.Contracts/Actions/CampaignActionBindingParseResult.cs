using Campaign.Contracts.Definitions;

namespace Campaign.Contracts.Actions;

public sealed record CampaignActionBindingParseResult(
    CampaignActionBindingConfig? Binding,
    object? Parameters,
    string? CanonicalJson,
    IReadOnlyList<CampaignDefinitionError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
