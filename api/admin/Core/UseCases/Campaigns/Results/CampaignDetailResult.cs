namespace Core.UseCases.Campaigns.Results;

public sealed record CampaignDetailResult(
    Guid CampaignId,
    string CampaignName,
    string? Description,
    string? BannerImageKey,
    string? BannerImageUrl,
    string EventType,
    DateTime StartDate,
    DateTime EndDate,
    string Condition,
    string? ScheduleCron,
    int? DurationHour,
    int? UserLimitTotal,
    int? UserLimitSession,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<CampaignActionResult> Actions,
    IReadOnlyCollection<CampaignSessionResult> Sessions,
    int SessionCount);

public sealed record CampaignActionResult(
    Guid ActionId,
    string ActionType,
    string ActionConfig,
    int ExecuteOrder,
    int? TotalCount,
    int? SessionCount,
    int UsedCount,
    DateTime CreatedAt);

public sealed record CampaignSessionResult(
    Guid CampaignSessionId,
    DateTime SessionStart,
    DateTime SessionEnd,
    string Status,
    DateTime CreatedAt,
    DateTime? EndedAt);
