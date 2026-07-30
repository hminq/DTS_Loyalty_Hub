using System.Text.Json;

namespace Api.Dtos.Requests.Campaigns;

public interface ICampaignWriteRequest
{
    string CampaignName { get; }
    string? Description { get; }
    string? BannerImageUrl { get; }
    Guid EventTypeVersionId { get; }
    JsonElement Condition { get; }
    DateTimeOffset StartDate { get; }
    DateTimeOffset EndDate { get; }
    string ScheduleCron { get; }
    int DurationHour { get; }
    int? UserLimitTotal { get; }
    int? UserLimitSession { get; }
}

/// <summary>Represents editable configuration for a draft campaign.</summary>
public sealed record CampaignWriteRequestDto : ICampaignWriteRequest
{
    public string CampaignName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? BannerImageUrl { get; init; }
    public Guid EventTypeVersionId { get; init; }
    public JsonElement Condition { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public string ScheduleCron { get; init; } = string.Empty;
    public int DurationHour { get; init; }
    public int? UserLimitTotal { get; init; }
    public int? UserLimitSession { get; init; }
}
