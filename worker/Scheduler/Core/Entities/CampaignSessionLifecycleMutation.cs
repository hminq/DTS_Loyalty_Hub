namespace Scheduler.Core.Entities;

public sealed record CampaignSessionLifecycleMutation(
    Guid CampaignSessionId,
    string TargetStatus,
    DateTime? EndedAt);
