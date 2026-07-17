namespace Api.Dtos.Requests.Notifications;

public sealed record GetNotificationTemplatesRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Keyword { get; init; }
    public string? EventTypeCode { get; init; }
    public string? Channel { get; init; }
    public string? Language { get; init; }
    public bool? IsActive { get; init; }
}
