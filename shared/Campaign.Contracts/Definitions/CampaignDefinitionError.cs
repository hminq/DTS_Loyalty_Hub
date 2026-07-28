namespace Campaign.Contracts.Definitions;

public sealed record CampaignDefinitionError(
    string Code,
    string Message,
    string? Path = null);
