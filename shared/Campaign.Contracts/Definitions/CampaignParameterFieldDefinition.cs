namespace Campaign.Contracts.Definitions;

public sealed record CampaignParameterFieldDefinition(
    string Code,
    string DataType,
    bool Required,
    decimal? MinimumExclusive = null,
    decimal? Maximum = null,
    int? Scale = null);
