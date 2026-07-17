namespace Api.Dtos.Requests.Notifications;

public sealed record GetEventTypesRequestDto
{
    public string? SearchKeyword { get; init; }
}
