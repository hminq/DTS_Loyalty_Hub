using Campaign.Contracts.Conditions;

namespace Campaign.Contracts.Definitions;

public sealed record CampaignTargetDefinition(
    string Selector,
    string TargetKind,
    CampaignCondition Applicability);
