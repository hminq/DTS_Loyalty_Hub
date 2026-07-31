namespace Api.Dtos.Requests.DevelopmentEvents;

public sealed class CustomerMissionCompletedRequestDto
{
    public Guid CustomerId { get; set; }

    public string MissionCode { get; set; } = null!;

    public string MissionCategory { get; set; } = null!;

    public bool? IsFirstCompletion { get; set; }

    public int CompletionCount { get; set; }
}
