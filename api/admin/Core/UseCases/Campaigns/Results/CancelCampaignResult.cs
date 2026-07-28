namespace Core.UseCases.Campaigns.Results;

public sealed record CancelCampaignResult(
    Guid CampaignId,
    string Status,
    int CancelledScheduledSessionCount,
    int CancelledRunningSessionCount,
    DateTime CancelledAt);
