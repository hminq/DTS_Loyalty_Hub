namespace Api.Dtos.Responses.Campaigns;

public sealed record CancelCampaignResponseDto(
    Guid CampaignId,
    string Status,
    int CancelledScheduledSessionCount,
    int CancelledRunningSessionCount,
    DateTimeOffset CancelledAt);
