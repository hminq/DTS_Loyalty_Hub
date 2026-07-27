namespace Core.UseCases.Campaigns.Commands;

public sealed record CreateCampaignActionInput(
    string ActionType,
    string ActionConfigJson,
    int ExecuteOrder,
    int? TotalCount,
    int? SessionCount,
    decimal? TotalAmount,
    decimal? SessionAmount);
