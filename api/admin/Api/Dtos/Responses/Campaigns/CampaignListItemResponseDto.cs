namespace Api.Dtos.Responses.Campaigns;

/// <summary>Represents one compact campaign list row.</summary>
public sealed record CampaignListItemResponseDto(
    Guid CampaignId,
    string CampaignName,
    string EventType,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? ScheduleCron,
    int? DurationHour,
    int ActionCount,
    DateTimeOffset? NextSessionStart,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
