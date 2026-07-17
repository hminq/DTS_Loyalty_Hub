using System;

namespace Api.Dtos.Requests.Notifications;

public sealed record GetNotificationLogsRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CustomerId { get; init; }
    public string? EventTypeCode { get; init; }
}
