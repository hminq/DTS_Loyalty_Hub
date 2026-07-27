namespace Campaign.Contracts.Campaigns.Actions;

public sealed record IssuePointActionConfig(
    string CalculationType,
    string Recipient,
    decimal? Amount,
    string? CalculationBase,
    decimal? Percentage,
    decimal? MaximumPoints);
