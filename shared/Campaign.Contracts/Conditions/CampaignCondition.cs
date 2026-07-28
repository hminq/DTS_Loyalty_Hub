namespace Campaign.Contracts.Conditions;

public sealed record CampaignCondition(IReadOnlyList<CampaignConditionPredicate> All)
{
    public static CampaignCondition MatchAll { get; } = new(Array.Empty<CampaignConditionPredicate>());
}
