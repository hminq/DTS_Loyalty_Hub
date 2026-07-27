namespace Api.Dtos.Requests.Campaigns;

/// <summary>Filters and paging values for the campaign list.</summary>
public sealed record GetCampaignsRequestDto
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? Keyword { get; init; }

    public string? Status { get; init; }

    public string? EventType { get; init; }
}
