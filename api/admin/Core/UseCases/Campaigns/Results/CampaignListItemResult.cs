namespace Core.UseCases.Campaigns.Results;

public sealed record CampaignListItemResult(
    Guid CampaignId,
    string CampaignName,
    CampaignEventDefinitionReferenceResult EventDefinition,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    string? ScheduleCron,
    int? DurationHour,
    int ActionCount,
    DateTime? NextSessionStart,
    DateTime CreatedAt,
    DateTime UpdatedAt);
