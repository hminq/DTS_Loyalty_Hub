using System.Text.Json;

namespace Api.Dtos.Requests.Campaigns;

/// <summary>Represents editable configuration for an action of a draft campaign.</summary>
public sealed record CampaignActionWriteRequestDto
{
    public string ActionType { get; init; } = string.Empty;
    public JsonElement ActionConfig { get; init; }
    public int ExecuteOrder { get; init; }
    public int? TotalCount { get; init; }
    public int? SessionCount { get; init; }
}
