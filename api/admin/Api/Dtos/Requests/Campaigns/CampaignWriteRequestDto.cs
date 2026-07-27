using System.Text.Json;

namespace Api.Dtos.Requests.Campaigns;

/// <summary>Represents editable configuration for a draft campaign.</summary>
public sealed record CampaignWriteRequestDto
{
    public string CampaignName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? BannerImageUrl { get; init; }
    public string EventType { get; init; } = string.Empty;
    public JsonElement Condition { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public string ScheduleCron { get; init; } = string.Empty;
    public int DurationHour { get; init; }
    public int? UserLimitTotal { get; init; }
    public int? UserLimitSession { get; init; }
}
