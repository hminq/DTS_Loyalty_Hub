namespace Campaign.Contracts.Definitions;

public sealed record CampaignActionDefinition(
    string Code,
    string RequiredTargetKind,
    IReadOnlyList<CampaignParameterFieldDefinition> Parameters);
