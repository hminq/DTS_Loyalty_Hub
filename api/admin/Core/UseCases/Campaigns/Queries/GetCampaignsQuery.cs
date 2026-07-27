using Core.UseCases.Campaigns.Results;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Campaigns.Queries;

public sealed record GetCampaignsQuery(
    int Page,
    int PageSize,
    string? Keyword,
    string? Status,
    string? EventType) : IRequest<PagedResult<CampaignListItemResult>>;
