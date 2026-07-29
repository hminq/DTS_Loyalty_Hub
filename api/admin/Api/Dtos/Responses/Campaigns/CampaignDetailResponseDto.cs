using System.Text.Json;

namespace Api.Dtos.Responses.Campaigns;

/// <summary>Represents complete campaign configuration with bounded session data.</summary>
public sealed record CampaignDetailResponseDto(
    Guid CampaignId,
    string CampaignName,
    string? Description,
    string? BannerImageKey,
    string? BannerImageUrl,
    CampaignEventDefinitionReferenceResponseDto EventDefinition,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    JsonElement Condition,
    string? ScheduleCron,
    int? DurationHour,
    int? UserLimitTotal,
    int? UserLimitSession,
    string Status,
    string ScheduleTimeZone,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<CampaignActionResponseDto> Actions,
    IReadOnlyCollection<CampaignSessionResponseDto> Sessions,
    int SessionCount);

/// <summary>Represents an ordered campaign action.</summary>
public sealed record CampaignActionResponseDto(
    Guid ActionId,
    string ActionType,
    JsonElement ActionConfig,
    int ExecuteOrder,
    int? TotalCount,
    int? SessionCount,
    int UsedCount,
    DateTimeOffset CreatedAt);

/// <summary>Represents one materialized campaign session.</summary>
public sealed record CampaignSessionResponseDto(
    Guid CampaignSessionId,
    DateTimeOffset SessionStart,
    DateTimeOffset SessionEnd,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EndedAt);
