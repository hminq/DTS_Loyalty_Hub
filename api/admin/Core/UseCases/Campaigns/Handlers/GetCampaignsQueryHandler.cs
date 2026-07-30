using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using Core.UseCases.Common;
using MediatR;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class GetCampaignsQueryHandler
    : IRequestHandler<GetCampaignsQuery, PagedResult<CampaignListItemResult>>
{
    private const int MaximumPageSize = 100;
    private readonly ICampaignRepository _campaignRepository;

    public GetCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public Task<PagedResult<CampaignListItemResult>> Handle(
        GetCampaignsQuery request,
        CancellationToken ct)
    {
        if (request.Page < 1)
        {
            throw new DomainException("PAGE_INVALID", DomainErrorType.Validation);
        }

        if (request.PageSize < 1 || request.PageSize > MaximumPageSize)
        {
            throw new DomainException("PAGE_SIZE_INVALID", DomainErrorType.Validation);
        }

        var status = NormalizeOptional(request.Status);
        if (status is not null && !CampaignStatuses.IsDefined(status))
        {
            throw new DomainException("CAMPAIGN_STATUS_INVALID", DomainErrorType.Validation);
        }

        return _campaignRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Keyword,
            status,
            request.EventTypeId,
            ct);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToUpperInvariant();
    }
}
