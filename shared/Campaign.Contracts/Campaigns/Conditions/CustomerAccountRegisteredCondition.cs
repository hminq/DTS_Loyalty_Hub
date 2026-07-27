namespace Campaign.Contracts.Campaigns.Conditions;

public sealed record CustomerAccountRegisteredCondition(
    IReadOnlyList<string>? Sources);
