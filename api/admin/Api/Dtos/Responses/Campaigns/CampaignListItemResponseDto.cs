namespace Api.Dtos.Responses.Campaigns;

/// <summary>Represents one compact campaign list row.</summary>
public sealed record CampaignListItemResponseDto(
    Guid CampaignId,
    string CampaignName,
    CampaignEventDefinitionReferenceResponseDto EventDefinition,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? ScheduleCron,
    int? DurationHour,
    int ActionCount,
    DateTimeOffset? NextSessionStart,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>Represents the event definition pinned by a campaign.</summary>
public sealed record CampaignEventDefinitionReferenceResponseDto(
    Guid EventTypeId,
    Guid EventTypeVersionId,
    string Code,
    string Name,
    int Version);
